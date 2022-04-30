using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAgentsManager : MonoBehaviour
{
    public static CharacterAgentsManager Instance;

    public CharacterAgentController player;
    public Dictionary<CharacterTeam, List<ITargettable>> characters;
    public Dictionary<string, ITargettable> charactersByID;
    public Dictionary<int, ITargettable> charactersByIndex;

    public List<ITargettable> allCharacters;
    private List<IMinable> allMinables;

    private List<ITargettable> charactersToDelete;

    public static void ValidateInstanceExists()
    {
        if (Instance == null || Instance.allCharacters == null)
        {
            Instance = GameObject.FindObjectOfType<CharacterAgentsManager>();
            if (Instance == null)
            {
                var go = new GameObject("CharacterAgentsManager");
                Instance = go.AddComponent<CharacterAgentsManager>();
            }

            Instance.allCharacters = new List<ITargettable>();
            Instance.allMinables = new List<IMinable>();
            Instance.characters = new Dictionary<CharacterTeam, List<ITargettable>>();
            Instance.charactersByID = new Dictionary<string, ITargettable>();
            Instance.charactersByIndex = new Dictionary<int, ITargettable>();
        }
    }

    public int playerIndex = 1;

    public static void SetPlayerCharacter(CharacterAgentController character)
    {
        ValidateInstanceExists();
        Instance.player = character;
    }

    public ITargettable GetCharacterByIndex(int index)
    {
        if (charactersByIndex.ContainsKey(index))
        {
            return charactersByIndex[index];
        }
        return null;
    }

    public static void AddCharacterAgent(CharacterAgentController character, string id)
    {
        ValidateInstanceExists();

        Instance.allCharacters.Add(character);
        Instance.AssignCharacterToTeam(character, character.team);
        if (!string.IsNullOrEmpty(id) && !Instance.charactersByID.ContainsKey(id))
            Instance.charactersByID.Add(id, character);

        
        character.battleTurnManager.playerIndex = Instance.playerIndex;
        if (!Instance.charactersByIndex.ContainsKey(Instance.playerIndex))
            Instance.charactersByIndex.Add(Instance.playerIndex, character);

        Instance.playerIndex++;

        Debug.Log("New Character Index : " + Instance.playerIndex.ToString());
    }

    public static void AddMinable(IMinable minable)
    {
        ValidateInstanceExists();
        Instance.allMinables.Add(minable);
    }

    public void AssignCharacterToTeam(CharacterAgentController character, CharacterTeam team)
    {
        if (!characters.ContainsKey(team))
        {
            characters.Add(team, new List<ITargettable>());
        }

        characters[team].Add(character);
    }

    public void RemoveCharacterTeamAssignment(CharacterAgentController character, CharacterTeam team)
    {
        if (characters.ContainsKey(team))
        {
            characters[team].Remove(character);
        }
    }

    public static void RemoveCharacterAgent(ITargettable character)
    {
        ValidateInstanceExists();
        if (Instance.allCharacters == null) return;
        if (Instance.charactersToDelete == null) Instance.charactersToDelete = new List<ITargettable>();
        Instance.charactersToDelete.Add(character);
        //Instance.allCharacters.Remove(character);
    }

    public static List<ITargettable> FindClosestTargettablesWithinRange(ITargettable targetter, Vector3 toPoint, float range)
    {
        List<ITargettable> targettables = new List<ITargettable>();
        ITargettable[] allTargettables = Instance.allCharacters.ToArray();

        foreach (ITargettable targettable in allTargettables)
        {
            if (targettable != targetter)
            {
                Vector3 delta = targettable.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < range)
                {
                    targettables.Add(targettable);
                }
            }
        }

        return targettables;
    }

    public static List<IAttackable> FindClosestAlliesWithinRange(Vector3 toPoint, float range, CharacterTeam team)
    {
        List<IAttackable> alliesInRange = new List<IAttackable>();
        ITargettable[] allyUnitsUnits = Instance.characters[team].ToArray();

        foreach (ITargettable ally in allyUnitsUnits)
        {
            IAttackable attackable = ally as IAttackable;
            if (attackable != null && (attackable.GetTeam() == team || team == CharacterTeam.ALL))
            {
                Vector3 delta = ally.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < range)
                {
                    alliesInRange.Add(attackable);
                }
            }
        }

        return alliesInRange;
    }

    public static List<IAttackable> FindClosestEnemiesWithinRange(Vector3 toPoint, float range, CharacterTeam team)
    {
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        List<IAttackable> enemiesInRange = new List<IAttackable>();

        foreach (ITargettable enemy in enemyUnits)
        {
            IAttackable attackable = enemy as IAttackable;
            if (attackable != null && (team == CharacterTeam.ALL || (attackable.GetTeam() != team && attackable.GetTeam() != CharacterTeam.NEUTRAL)))
            {
                Vector3 delta = enemy.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < range)
                {
                    enemiesInRange.Add(attackable);
                }
            }
        }

        return enemiesInRange;
    }

    public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
    {
        var line = (end - start);
        var len = line.magnitude;
        line.Normalize();

        var v = pnt - start;
        var d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return start + line * d;
    }

    public static Vector3 FindNearestRaycastTarget(Vector3 start, Vector3 end, float radius, CharacterTeam team)
    {
        float y = end.y;
        start.y = 0;
        end.y = 0;

        float nearestDistance = Vector3.Distance(start, end);
        Vector3 target = end;
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        List<IAttackable> enemyUnitsInRange = new List<IAttackable>();
        foreach (ITargettable enemy in enemyUnits)
        {
            IAttackable attackable = enemy as IAttackable;
            if (attackable != null && (attackable.GetTeam() != team || team == CharacterTeam.ALL))
            {
                var ePos = enemy.GetPosition();

                Vector3 closestPointToLine = NearestPointOnFiniteLine(start, end, ePos);
                float distance = Vector3.Distance(closestPointToLine, ePos);
                if (distance < (radius + attackable.GetColliderRadius()) && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    target = closestPointToLine;
                }
            }
        }
        target.y = y;
        return target;
    }

    public static List<IAttackable> FindAttackableEnemiesWithinRangeOfLine(Vector3 start, Vector3 end, float radius, CharacterTeam team)
    {
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        List<IAttackable> enemyUnitsInRange = new List<IAttackable>();
        foreach (ITargettable enemy in enemyUnits)
        {
            IAttackable attackable = enemy as IAttackable;
            if (attackable != null && (attackable.GetTeam() != team || team == CharacterTeam.ALL))
            {
                Vector3 closestPointToLine = NearestPointOnFiniteLine(start, end, attackable.GetPosition());
                float distance = Vector3.Distance(closestPointToLine, attackable.GetPosition());
                if (attackable.GetCollision(closestPointToLine, radius))
                //if (distance < (radius + attackable.GetColliderRadius()))
                {
                    enemyUnitsInRange.Add(attackable);
                }
            }
        }
        return enemyUnitsInRange;
    }

    public static List<IMinable> FindMinablesWihtinRange(Vector3 toPoint, float range)
    {
        IMinable[] minableUnits = Instance.allMinables.ToArray();
        List<IMinable> closestMinables = new List<IMinable>();
        foreach (IMinable enemy in minableUnits)
        {
            Vector3 delta = enemy.GetPosition() - toPoint; delta.y = 0;
            float distance = delta.magnitude;
            if (distance <= range)
            {
                closestMinables.Add(enemy);
            }
        }

        return closestMinables;
    }

    public static ITargettable FindClosestAttackableEnemy(Vector3 toPoint)
    {
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        float closestDistance = float.MaxValue;
        ITargettable closestEnemy = null;
        foreach (ITargettable enemy in enemyUnits)
        {
            Vector3 delta = enemy.GetPosition() - toPoint; delta.y = 0;
            float distance = delta.magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
            
        }
        if (closestEnemy == null)
        {
            return null;
        }

        return closestEnemy;
    }

    public static ITargettable FindClosestTargettable(IAbilityCaster targetter, Vector3 toPoint)
    {
        ITargettable[] targettables = Instance.allCharacters.ToArray();
        float closestDistance = float.MaxValue;
        ITargettable closestTargettable = null;
        foreach (ITargettable targettable in targettables)
        {
            if (targettable != targetter)
            {
                Vector3 delta = targettable.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTargettable = targettable;
                }
            }
        }
        if (closestTargettable == null)
        {
            return null;
        }

        return closestTargettable;
    }

    public static IAttackable FindClosestAttackableEnemy(Vector3 toPoint, CharacterTeam team)
    {
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        float closestDistance = float.MaxValue;
        IAttackable closestEnemy = null;
        foreach (ITargettable enemy in enemyUnits)
        {
            IAttackable attackable = enemy as IAttackable;
            if (attackable.GetTeam() != team && attackable.GetTeam() != CharacterTeam.NEUTRAL)
            {
                Vector3 delta = enemy.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = attackable;
                }
            }
        }
        if (closestEnemy == null)
        {
            return null;
        }

        return closestEnemy;
    }

    public static ITargettable FindClosestTargettableEnemy(Vector3 toPoint, CharacterTeam team)
    {
        ITargettable[] enemyUnits = Instance.allCharacters.ToArray();
        float closestDistance = float.MaxValue;
        ITargettable closestEnemy = null;
        foreach (ITargettable enemy in enemyUnits)
        {
            IAttackable attackable = enemy as IAttackable;
            if (attackable.GetTeam() != team && attackable.GetTeam() != CharacterTeam.NEUTRAL)
            {
                Vector3 delta = enemy.GetPosition() - toPoint; delta.y = 0;
                float distance = delta.magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        if (closestEnemy == null)
        {
            return null;
        }

        return closestEnemy;
    }

    public void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("CharacterManager reporting");
    }

    // Update is called once per frame
    void Update()
    {
        if (allCharacters == null) return;

        if (charactersToDelete != null && charactersToDelete.Count > 0)
        {
            foreach (ITargettable agent in charactersToDelete)
            {
                allCharacters.Remove(agent);
            }
            charactersToDelete.Clear();
        }

        foreach (ITargettable agent in allCharacters)
        {
            agent?.Tick();
        }

        foreach (IMinable minable in allMinables)
        {
            minable?.Tick();
        }
    }
}
