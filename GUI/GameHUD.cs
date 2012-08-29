using UnityEngine;
using System.Collections;
using System;

public class GameHUD : MonoBehaviour
{
	public GUISkin guiSkin;
	public float nativeVerticalResolution = 1200.0f;
	
	// main decoration textures:
	public Texture2D healthImage;
	public Vector2 healthImageOffset = new Vector2(0, 0);
	
	// the health 'pie chart' assets consist of six textures with alpha channels. Only one is ever shown:
	public Texture2D[] healthPieImages;
	public Vector2 healthPieImageOffset = new Vector2(10, 147);
	
	// the lives count is displayed in the health image as a text counter
	public Vector2 livesCountOffset = new Vector2(425, 160);
	
	// The fuel cell decoration image on the right side
	public Texture2D fuelCellsImage;
	public Vector2 fuelCellOffset = new Vector2(0, 0);
	
	// The counter text inside the fuel cell image
	public Vector2 fuelCellCountOffset = new Vector2(391, 161);
	
	private ThirdPersonStatus playerInfo;


	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	
	// Cache link to player's state management script for later use.
	void Awake()
	{
		playerInfo = (ThirdPersonStatus)FindObjectOfType(typeof(ThirdPersonStatus));
	
		if (!playerInfo)
			Debug.Log("No link to player's state manager.");
	}
	
	void OnGUI ()
	{
	
		var itemsLeft = playerInfo.GetRemainingItems();	// fetch items remaining -- the fuel cans. This can be a negative number!
	
		// Similarly, health needs to be clamped to the number of pie segments we can show.
		// We also need to check it's not negative, so we'll use the Mathf Clamp() function:
		var healthPieIndex = Mathf.Clamp(playerInfo.health, 0, healthPieImages.Length);
	
		// Displays fuel cans remaining as a number.	
		// As we don't want to display negative numbers, we clamp the value to zero if it drops below this:
		if (itemsLeft < 0)
			itemsLeft = 0;
	
		// Set up gui skin
		GUI.skin = guiSkin;
	
		// Our GUI is laid out for a 1920 x 1200 pixel display (16:10 aspect). The next line makes sure it rescales nicely to other resolutions.
		GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (Screen.height / nativeVerticalResolution, Screen.height / nativeVerticalResolution, 1)); 
	
		// Health & lives info.
		DrawImageBottomAligned( healthImageOffset, healthImage); // main image.
	
		// now for the pie chart. This is where a decent graphics package comes in handy to check relative sizes and offsets.
		var pieImage = healthPieImages[healthPieIndex-1];
		DrawImageBottomAligned( healthPieImageOffset, pieImage );
		
		// Displays lives left as a number.	
		DrawLabelBottomAligned( livesCountOffset, playerInfo.lives.ToString() );	
		
		// Now it's the fuel cans' turn. We want this aligned to the lower-right corner of the screen:
		DrawImageBottomRightAligned( fuelCellOffset, fuelCellsImage);
	
		DrawLabelBottomRightAligned( fuelCellCountOffset, itemsLeft.ToString() );
	}
	
	void DrawImageBottomAligned (Vector2 pos, Texture2D image)
	{
		GUI.Label(new Rect (pos.x, nativeVerticalResolution - image.height - pos.y, image.width, image.height), image);
	}
	
	void DrawLabelBottomAligned (Vector2 pos, String text)
	{
		GUI.Label(new Rect (pos.x, nativeVerticalResolution - pos.y, 100, 100), text);
	}
	
	void DrawImageBottomRightAligned (Vector2 pos, Texture2D image)
	{
		var scaledResolutionWidth = nativeVerticalResolution / Screen.height * Screen.width;
		GUI.Label(new Rect (scaledResolutionWidth - pos.x - image.width, nativeVerticalResolution - image.height - pos.y, image.width, image.height), image);
	}
	
	void DrawLabelBottomRightAligned (Vector2 pos, String text)
	{
		var scaledResolutionWidth = nativeVerticalResolution / Screen.height * Screen.width;
		GUI.Label(new Rect (scaledResolutionWidth - pos.x, nativeVerticalResolution - pos.y, 100, 100), text);
	}	
}

