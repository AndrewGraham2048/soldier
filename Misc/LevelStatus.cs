using UnityEngine;
using System.Collections;
using System;

public class LevelStatus : MonoBehaviour
{
	public int itemsNeeded = 20;	// This is how many fuel canisters the player must collect.
	
	//var blockPrefabs:GameObject[] = new GameObject[2]; //Lets say you have only 2 different blocks.
	//var lastBlock:GameObject; // Store the last/current block. Remember that the first block shoul be in scene and assigned to this variable.
	public Transform bulletPrefab;
	public Transform enemyPrefab;
	public Transform[] tilePrefabs;
	public Transform roundPrefab;
	
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	
	void Awake()
	{
	//    var insPos:Vector3 = Vector3(100,100,100);
	//    var newBlock:GameObject = Instantiate(blockPrefabs[Mathf.Floor(Random.Range(0,2))],insPos, lastBlock.transform.rotation);
	    Application.targetFrameRate = -1;	//60;
	
	    //var levelArray : String[];
	    var levelArray = new String[40];
	    levelArray[0] = "1111111111111111111111111111111111111111";
	    levelArray[1] = "2000000030000000500000000000000000000002";
	    levelArray[2] = "2005000000000000000000000000000000400002";
	    levelArray[3] = "2000000222230222230222222222222000000002";
	    levelArray[4] = "2000030000000000000000000000400000000002";
	    levelArray[5] = "2000000000040000005000000000000000500002";
	    levelArray[6] = "2050000000000030000000000000000000000002";
	    levelArray[7] = "2000000030000000000222222222222222222222";
	    levelArray[8] = "2003000000000000000000000000000000000002";
	    levelArray[9] = "2000000000000030000000050000000000000002";
	    levelArray[10] = "2000000040000000000000000000000000000002";
	    levelArray[11] = "2000000000000000000000005001111111111112";
	    levelArray[12] = "2000300000500000000300000000000000000002";
	    levelArray[13] = "2000000000003000000000000030000000000002";
	    levelArray[14] = "2000000000000000000000000000000000300002";
	    levelArray[15] = "2000000000000000000000000000000000000002";
	    levelArray[16] = "2222222225001111111111111111122222222222";
	    levelArray[17] = "2000000000000000000003000000000000000002";
	    levelArray[18] = "2003000000000050000000000000000030000002";
	    levelArray[19] = "2000000000000000000000500000000000000002";
	    levelArray[20] = "2222222222222222222220002222222222222222";
	    levelArray[21] = "2000000000000000000400003000000000000002";
	    levelArray[22] = "2003000000500000300000000000000111140002";
	    levelArray[23] = "2000000000000000000000000005000000000002";
	    levelArray[24] = "2222222222222111112220022222222222222222";
	    levelArray[25] = "2000000000000000000000500000400000000002";
	    levelArray[26] = "2000030040000000500000003000000500030002";
	    levelArray[27] = "2000000000000000000000000000000000000002";
	    levelArray[28] = "2402222220500222222222222222222000222222";
	    levelArray[29] = "2000000000000300000050000000000000030002";
	    levelArray[30] = "2000000000000000000000030000000000000002";
	    levelArray[31] = "2000000000000000000000000000000000000002";
	    levelArray[32] = "2222222222225004000502222222222222222222";
	    levelArray[33] = "2004000000000000000000000000000000000002";
	    levelArray[34] = "2020002222000000000001110000000001111112";
	    levelArray[35] = "2000030000000000000000012222220000300002";
	    levelArray[36] = "2000000003000000000000000000000000000002";
	    levelArray[37] = "2000000000000000000011111300000000000002";
	    levelArray[38] = "2000000000000000000000000000000000000002";
	    levelArray[39] = "1111111111111111111111111111111111111111";
	                         
	    var root = new GameObject();
	    
	    for (int x = 0; x < 40; x++)
	   	{ 
	    	for (int y = 0; y < 40; y++) 
	    	{
	    		String s = levelArray[(39-(y%40))];
	    		int i;
	    		i = s[x%40];
	    		i -= 48;
	    		var a = i;
	    		var pos = new Vector3(-118.0f+(2*x), 5.9f, -1.0f+(2*y));
	    		//Instantiate (enemyPrefab, pos + Vector3(0,1,0), Quaternion.identity);
				Transform thing;
	      		if (a == 5)
	    		{
	    			thing = (Transform)Instantiate (enemyPrefab, pos + new Vector3(0,1,0), Quaternion.identity);
	    		}
	    		else if (a > 0)
	    		{
	    			if ((a==3) || (a==4))
	    				thing = (Transform)Instantiate (tilePrefabs[a-1], pos + new Vector3(1,0,-1), Quaternion.Euler(270,0,0));
	    			else
	    				thing = (Transform)Instantiate (tilePrefabs[a-1], pos, Quaternion.Euler(270,0,0));
	
	    			//thing.isStatic = true;
					thing.transform.parent = root.transform;
	       		}
			}
		}
		StaticBatchingUtility.Combine(root);
	}	
	
	// Update is called once per frame
	void Update ()
	{
		foreach (Touch evt in Input.touches)
		{
			if (evt.phase == TouchPhase.Began)
			{
	        	GameObject p;
	        	p = GameObject.Find("player");
	        	var rot = Quaternion.identity;	// p.transform.rotation;
	        	Transform bullet = (Transform)Instantiate( bulletPrefab, p.transform.position + new Vector3(0,1,0), rot );
			}
		}
	}
}

