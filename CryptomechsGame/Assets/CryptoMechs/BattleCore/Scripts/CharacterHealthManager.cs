using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterHealthManager 
{
    public CharacterHealthManager(CharacterAgentController character)
    {
        InitializeHealthBar(character);
    }

    private CharacterAgentController cachedCharacter;
    public SimpleHealthBar healthBar;
    public TextMeshProUGUI hpText;

    public GameObject healthUI;
    private Color healthBarColor;

    public float healthPoints = 500;
    public float healthPointsTotal = 1000;
    public bool IsDead { get { return healthPoints <= 0; } }

    private SkinnedMeshRenderer mesh;

    public void UpdateHealthBar(int healthAmount, int maxHealth, bool isHeal = false)
    {
        healthBar.UpdateBar(healthAmount, maxHealth);

        cachedCharacter.StartCoroutine(FlashCharacter(isHeal));

        //if (healthAmount <= 0)
        //{
            //if (ParticleFXManager.Instance != null)
            //{
                //GameObject bloodFX = ParticleFXManager.Instance.spawnParticleFX(12);
                //Vector3 targetFXpos = transform.position; // + new Vector3(0, 0.1f, 0);
                //bloodFX.transform.position += targetFXpos;
            //}
        //}
    }

    public void InitializeHealthBar(CharacterAgentController character)
    {
        cachedCharacter = character;

        // initialize health bar
        if (character.team != CharacterTeam.OPPONENT)
        {
            healthUI = GameObject.Instantiate(Resources.Load("Prefabs/HUD/HealthCanvasGreen") as GameObject);
        }
        else
        {
            healthUI = GameObject.Instantiate(Resources.Load("Prefabs/HUD/HealthCanvas") as GameObject);
        }

        if (healthUI != null)
        {
            healthUI.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            Vector3 uiPos = cachedCharacter.GetHeadVisuals().transform.position + new Vector3(0, 1.0f, 0);
            healthUI.transform.position = uiPos;
            healthUI.transform.SetParent(cachedCharacter.GetCharacterVisuals().transform);
            healthBar = healthUI.GetComponentInChildren<SimpleHealthBar>();
            
            hpText = healthUI.GetComponentInChildren<TextMeshProUGUI>();

            healthBarColor = healthBar.GetComponent<Image>().color;

            if (healthBar != null)
            {
                healthBar.UpdateBar(healthPoints, healthPointsTotal);
            }
        }

        this.healthPointsTotal = character.data.MaxHealth;
        this.healthPoints = this.healthPointsTotal;
        
        mesh = character.GetCharacterVisuals().GetComponentInChildren<SkinnedMeshRenderer>();
    }

    IEnumerator FlashCharacter(bool isHeal = false)
    {
        if (mesh == null || healthBar == null) yield break;

        float flashTime = isHeal ? 0.3f : 0.17f;

        // character color flash 
        if (!isHeal)
        {
            mesh.material.color = new Color(1.7f, 1.4f, 1.4f);
        }
        else
        {
            mesh.material.color = new Color(1.4f, 1.8f, 1.4f);
        }

        Image img = healthBar.GetComponent<Image>();
        Color color = img.color;
        Color flashColor = color + Color.white;
        img.color = flashColor;

        yield return new WaitForSeconds(flashTime);

        if (healthBar != null)
        {
            img.color = healthBarColor;
        }

        if (mesh != null)
        {
            mesh.material.color = new Color(1, 1, 1);
        }
    }

    public void TakeDamage(float damagePoints)
    {
        //return;
        if (IsDead)
        {
            return;
        }

        healthPoints -= damagePoints;
        if (IsDead)
        {
            cachedCharacter.TriggerDeath();

            GameObject.Destroy(healthUI);
            //GameObject.Destroy(this.gameObject, 2.5f);

            //if (ParticleFXManager.Instance != null)
            //{
            //    GameObject bloodFX = ParticleFXManager.Instance.spawnParticleFX(12);
            //    Vector3 targetFXpos = cachedCharacter.GetCharacterVisuals().transform.position; // + new Vector3(0, 0.1f, 0);
            //    bloodFX.transform.position += targetFXpos;
            //}

        }

        if (healthBar != null)
            healthBar.UpdateBar(healthPoints, healthPointsTotal);

        cachedCharacter.StartCoroutine(FlashCharacter());

        if (hpText != null)
        {
            hpText.text = healthPoints.ToString();
        }

        //spawn damage particle fx
        return;
    }

    public void UpdateHealth()
    {
        if (healthBar != null)
            healthBar.UpdateBar(healthPoints, healthPointsTotal);

        cachedCharacter.StartCoroutine(FlashCharacter());

        if (hpText != null)
        {
            hpText.text = healthPoints.ToString();
        }
    }

}
