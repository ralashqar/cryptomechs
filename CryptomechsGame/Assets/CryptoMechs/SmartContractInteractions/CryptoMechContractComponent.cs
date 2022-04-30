using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CryptoMechContractComponent : MonoBehaviour
{
    public string chain = "ethereum";
    public string network = "rinkeby";
    public string contract = "0x72f597E452495C9b929B5Ea0CF9Ffa35Ee815c44";
    public string account = "0xbE519500010b11035423A252a9aCA32247230F59";
    public string tokenId = "1";

    public int balance = 0;
    public bool isComputingBalance = false;

    public void Start()
    {
        CreatePart("p5", 4);    
    }

    public async void GetBalanceOf(string account, string tokenId)
    {
        isComputingBalance = true;
        BigInteger balanceOf = await ERC1155.BalanceOf(chain, network, contract, this.account, tokenId);
        print(balanceOf);
        balance = (int)balanceOf;
        isComputingBalance = false;
    }

    public async void CreatePart(string partID, int numParts)
    {
        try 
        {
            //string response = await Web3GL.Send(method, abi, contract, args, value);
            //Debug.Log(response);
        } 
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }




    //Merge Parts
    // smart contract method to call
    string method = "createpart";
    // abi in json format
    //string abi = "[ { \"inputs\": [], \"name\": \"increment\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [], \"name\": \"x\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"stateMutability\": \"view\", \"type\": \"function\" } ]";
    // address of contract
    // array of arguments for contract
    string args = "[\"p5\", 3]";
    // value in wei
    string value = "0";

    // connects to user's browser wallet (metamask) to send a transaction
    //try {
    //        string response = await Web3GL.Send(method, abi, contract, args, value);
    //Debug.Log(response);
    //    } catch (Exception e)
    //{
    //    Debug.LogException(e, this);
    //}

    string abi = "[ { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_owner\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_operator\", \"type\": \"address\" }, { \"indexed\": false, \"internalType\": \"bool\", \"name\": \"_approved\", \"type\": \"bool\" } ], \"name\": \"ApprovalForAll\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"previousOwner\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\" } ], \"name\": \"OwnershipTransferred\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_operator\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_from\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"indexed\": false, \"internalType\": \"uint256[]\", \"name\": \"_ids\", \"type\": \"uint256[]\" }, { \"indexed\": false, \"internalType\": \"uint256[]\", \"name\": \"_amounts\", \"type\": \"uint256[]\" } ], \"name\": \"TransferBatch\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_operator\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_from\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"indexed\": false, \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" }, { \"indexed\": false, \"internalType\": \"uint256\", \"name\": \"_amount\", \"type\": \"uint256\" } ], \"name\": \"TransferSingle\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": false, \"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\" }, { \"indexed\": true, \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" } ], \"name\": \"URI\", \"type\": \"event\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"internalType\": \"uint256[]\", \"name\": \"_ids\", \"type\": \"uint256[]\" }, { \"internalType\": \"uint256[]\", \"name\": \"_quantities\", \"type\": \"uint256[]\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"batchMint\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_initialOwner\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"_initialSupply\", \"type\": \"uint256\" }, { \"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"create\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"string\", \"name\": \"partID\", \"type\": \"string\" }, { \"internalType\": \"uint256\", \"name\": \"numParts\", \"type\": \"uint256\" } ], \"name\": \"createpart\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"uint256[]\", \"name\": \"pIDs\", \"type\": \"uint256[]\" }, { \"internalType\": \"uint256[]\", \"name\": \"values\", \"type\": \"uint256[]\" } ], \"name\": \"mergeparts\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" }, { \"internalType\": \"uint256\", \"name\": \"_quantity\", \"type\": \"uint256\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"mint\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [], \"name\": \"renounceOwnership\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_from\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"internalType\": \"uint256[]\", \"name\": \"_ids\", \"type\": \"uint256[]\" }, { \"internalType\": \"uint256[]\", \"name\": \"_amounts\", \"type\": \"uint256[]\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"safeBatchTransferFrom\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_from\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" }, { \"internalType\": \"uint256\", \"name\": \"_amount\", \"type\": \"uint256\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"safeTransferFrom\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_operator\", \"type\": \"address\" }, { \"internalType\": \"bool\", \"name\": \"_approved\", \"type\": \"bool\" } ], \"name\": \"setApprovalForAll\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"string\", \"name\": \"_newBaseMetadataURI\", \"type\": \"string\" } ], \"name\": \"setBaseMetadataURI\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_to\", \"type\": \"address\" }, { \"internalType\": \"uint256[]\", \"name\": \"_ids\", \"type\": \"uint256[]\" } ], \"name\": \"setCreator\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"mechID\", \"type\": \"uint256\" } ], \"name\": \"splitparts\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"constant\": false, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\" } ], \"name\": \"transferOwnership\", \"outputs\": [], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_proxyRegistryAddress\", \"type\": \"address\" } ], \"payable\": false, \"stateMutability\": \"nonpayable\", \"type\": \"constructor\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_owner\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" } ], \"name\": \"balanceOf\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"address[]\", \"name\": \"_owners\", \"type\": \"address[]\" }, { \"internalType\": \"uint256[]\", \"name\": \"_ids\", \"type\": \"uint256[]\" } ], \"name\": \"balanceOfBatch\", \"outputs\": [ { \"internalType\": \"uint256[]\", \"name\": \"\", \"type\": \"uint256[]\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [], \"name\": \"contractURI\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"name\": \"creators\", \"outputs\": [ { \"internalType\": \"address\", \"name\": \"\", \"type\": \"address\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"address\", \"name\": \"_owner\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"_operator\", \"type\": \"address\" } ], \"name\": \"isApprovedForAll\", \"outputs\": [ { \"internalType\": \"bool\", \"name\": \"isOperator\", \"type\": \"bool\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [], \"name\": \"isOwner\", \"outputs\": [ { \"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [], \"name\": \"name\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [], \"name\": \"owner\", \"outputs\": [ { \"internalType\": \"address\", \"name\": \"\", \"type\": \"address\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"bytes4\", \"name\": \"_interfaceID\", \"type\": \"bytes4\" } ], \"name\": \"supportsInterface\", \"outputs\": [ { \"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [], \"name\": \"symbol\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"name\": \"tokenSupply\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" } ], \"name\": \"totalSupply\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" }, { \"constant\": true, \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"_id\", \"type\": \"uint256\" } ], \"name\": \"uri\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"payable\": false, \"stateMutability\": \"view\", \"type\": \"function\" } ]"
        ;
}
