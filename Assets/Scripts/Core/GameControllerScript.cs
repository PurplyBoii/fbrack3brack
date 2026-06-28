using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameControllerScript : MonoBehaviour
{
	private void Start()
	{
		cullingMask = playerCamera.cullingMask; // Changes cullingMask in the Camera
		audioDevice = GetComponent<AudioSource>(); //Get the Audio Source
		mode = PlayerPrefs.GetString("CurrentMode"); //Get the current mode
		if (mode == "endless") //If it is endless mode
		{
			baldiScrpt.endless = true; //Set Baldi use his slightly changed endless anger system
		}
		schoolMusic.Play(); //Play the school music
		MouseLock(true); //Prevent the mouse from moving
		UpdateNotebookCount(); //Update the notebook count
		itemSelected = 0; //Set selection to item slot 0(the first item slot)
		gameOverDelay = 0.5f;
	}
	private void Update()
	{
		if (ok > 0f)
		{
			ok -= Time.deltaTime * 1.5f;
			Color yo = popup.color;
			yo.a = ok;
			popup.color = yo;
		}

		if (!learningActive)
		{
			if (Singleton<InputManager>.Instance.GetActionKeyDown(InputAction.PauseOrCancel) && !player.gameOver)
			{
				if (!gamePaused)
				{
					Pause(true);
				}
				else
				{
					Pause(false);
				}
			}
			
			if (gamePaused)
			{
				if (Input.GetKeyDown(KeyCode.Y))
				{
					ExitGame();
				}
				else if (Input.GetKeyDown(KeyCode.N))
				{
					Pause(false);
				}
			}
			else
			{
				if (Time.timeScale != 1f)
				{
					Time.timeScale = 1f;
				}
			}

			if (Time.timeScale != 0f)
			{
				if (Singleton<InputManager>.Instance.GetActionKeyDown(InputAction.UseItem))
				{
					UseItem();
				}

				if (Input.GetAxis("Mouse ScrollWheel") > 0f)
				{
					ItemSelect(false);
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
				{
					ItemSelect(true);
				}

				if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Slot0))
				{
					itemSelected = 0;
					UpdateItemSelection();
				}
				else if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Slot1))
				{
					itemSelected = 1;
					UpdateItemSelection();
				}
				else if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Slot2))
				{
					itemSelected = 2;
					UpdateItemSelection();
				}
			}
		}
		else
		{
			if (Time.timeScale != 0f)
			{
				Time.timeScale = 0f;
			}
		}

		if (player.stamina < 0f & !warning.activeSelf)
		{
			warning.SetActive(true); //Set the warning text to be visible
		}
		else if (player.stamina > 0f & warning.activeSelf)
		{
			warning.SetActive(false); //Set the warning text to be invisible
		}

		if (player.gameOver)
		{
			if (mode == "endless" && notebooks > PlayerPrefs.GetInt("HighBooks") && !highScoreText.activeSelf)
			{
				highScoreText.SetActive(true);
			}

			Time.timeScale = 0f;
			gameOverDelay -= Time.unscaledDeltaTime * 0.5f;
			playerCamera.farClipPlane = gameOverDelay * 400f; //Set camera farClip 
			audioDevice.PlayOneShot(sfx[2]);

			if (gameOverDelay <= 0f)
			{
				if (mode == "endless")
				{
					if (notebooks > PlayerPrefs.GetInt("HighBooks"))
					{
						PlayerPrefs.SetInt("HighBooks", notebooks);
					}
					PlayerPrefs.SetInt("CurrentBooks", notebooks);
				}
				Time.timeScale = 1f;
				SceneManager.LoadScene(GameOverScene);
			}
		}

		if (finaleMode && !audioDevice.isPlaying && exitsReached == 3)
		{
			audioDevice.clip = CRAZYESCAPE[3];
			audioDevice.loop = true;
			audioDevice.Play();
		}
	}
	private void UpdateNotebookCount()
	{
		if (mode == "story")
		{
			notebookCount.text = notebooks.ToString() + "/8 Notebooks";
			if (notebooks == 3)
			{
				ActivateSpoopMode();
			}
			if (notebooks == 8)
			{
				ActivateFinaleMode();
			}
		}
		else
		{
			notebookCount.text = notebooks.ToString() + " Notebooks";
		}
	}
	public void CollectNotebook()
	{
		player.stamina = 100f;
		notebooks++;
		ok = 1f;
		popup.gameObject.GetComponent<AudioSource>().Play();
		UpdateNotebookCount();
	}
	public void MouseLock(bool lok)
	{
		if (lok)
		{
			if (!learningActive)
			{
				cursorController.LockCursor(); //Prevent the cursor from moving
				mouseLocked = true;
				reticle.SetActive(true);
			}
		}
		else
		{
			cursorController.UnlockCursor(); //Allow the cursor to move
			mouseLocked = false;
			reticle.SetActive(false);
		}
	}
	public void Pause(bool pause)
	{
		gamePaused = pause;
		pauseMenu.SetActive(pause);
		if (pause)
		{
			if (!learningActive)
			{
				MouseLock(false);
				Time.timeScale = 0f;
			}
		}
		else
		{
			Time.timeScale = 1f;
			MouseLock(true);
		}
	}
	public void ExitGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(ExitGameScene);
	}
	public void ActivateSpoopMode()
	{
		spoopMode = true; //Tells the game its time for spoop
		foreach (EntranceScript e in entrances)
		{
			e.Lower(); //Lowers all the exits
		}
		baldiTutor.SetActive(false); //Turns off Baldi(The one that you see at the start of the game)
		foreach (GameObject npc in spoopNpcs)
		{
			npc.SetActive(true);
		}
		//TestEnemy.SetActive(true); //Turns on Test-Enemy (Bonus)
		audioDevice.PlayOneShot(sfx[3]); //Plays the hang sound
		learnMusic.Stop(); //Stop all the music
		schoolMusic.Stop();
	}
	private void ActivateFinaleMode()
	{
		finaleMode = true;
		foreach (EntranceScript e in entrances)
		{
			e.Raise(); //Raise all the enterances(make them appear)
		}
	}
	public void GetAngry(float value) //Make Baldi get angry
	{
		if (!spoopMode)
		{
			ActivateSpoopMode();
		}
		baldiScrpt.GetAngry(value);
	}
	public void LearnGame(GameObject sub, bool activate)
	{
		if (activate)
		{
			//camera.cullingMask = 0; //Sets the cullingMask to nothing
			learningActive = true;
			MouseLock(false); //Unlock the mouse
			tutorBaldi.Stop(); //Make tutor Baldi stop talking
			if (!spoopMode) //If the player hasn't gotten a question wrong
			{
				schoolMusic.Stop(); //Start playing the learn music
				learnMusic.Play();
			}
		}
		else
		{
			playerCamera.cullingMask = cullingMask; //Sets the cullingMask to Everything
			learningActive = false;
			Destroy(sub);
			MouseLock(true); //Prevent the mouse from moving
			if (player.stamina < 100f) //Reset Stamina
			{
				player.stamina = 100f;
			}
			if (!spoopMode) //If it isn't spoop mode, play the school music
			{
				schoolMusic.Play();
				learnMusic.Stop();
			}
			if (notebooks == 1 & !spoopMode) // If this is the players first notebook and they didn't get any questions wrong, reward them with a quarter
			{
				quarter.SetActive(true);
				tutorBaldi.PlayOneShot(aud_Prize);
			}
			else if (notebooks == 8 & mode == "story") // Plays the all 7 notebook sound
			{
				audioDevice.PlayOneShot(aud_AllNotebooks, 0.8f);
			}
		}
	}
	void ItemSelect(bool up)
	{
		if (up)
		{
			itemSelected++;
			if (itemSelected > 2)
			{
				itemSelected = 0;
			}
		}
		else
		{
			itemSelected--;
			if (itemSelected < 0)
			{
				itemSelected = 2;
			}
		}
		itemSelect.anchoredPosition = new Vector3(itemSelectOffset[itemSelected], (itemSelectPosition)); //Moves the item selector background(the red rectangle)
		UpdateItemName();
	}
	private void UpdateItemSelection()
	{
		itemSelect.anchoredPosition = new Vector3(itemSelectOffset[itemSelected], (itemSelectPosition)); //Moves the item selector background(the red rectangle)
		UpdateItemName();
	}
	public void CollectItem(int item_ID)
	{
		if (item[0] == 0)
		{
			item[0] = item_ID; //Set the item slot to the Item_ID provided
			itemSlot[0].texture = itemTextures[item_ID]; //Set the item slot's texture to a texture in a list of textures based on the Item_ID
		}
		else if (item[1] == 0)
		{
			item[1] = item_ID; //Set the item slot to the Item_ID provided
            itemSlot[1].texture = itemTextures[item_ID]; //Set the item slot's texture to a texture in a list of textures based on the Item_ID
        }
		else if (item[2] == 0)
		{
			item[2] = item_ID; //Set the item slot to the Item_ID provided
            itemSlot[2].texture = itemTextures[item_ID]; //Set the item slot's texture to a texture in a list of textures based on the Item_ID
        }
		else //This one overwrites the currently selected slot when your inventory is full
		{
			item[itemSelected] = item_ID;
			itemSlot[itemSelected].texture = itemTextures[item_ID];
		}
		UpdateItemName();
	}
	private void UseItem()
	{
		int it = item[itemSelected];
		Ray ray = Camera.main.ScreenPointToRay(new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2), 0f));
		RaycastHit raycastHit;
		if (it != 0)
		{
			if (it == 1)
			{
				player.stamina = player.maxStamina * 2f;
				ResetItem();
				//player.ResetGuilt("food", 3f);
			}
			else if (it == 2)
			{
				if (Physics.Raycast(ray, out raycastHit) && (raycastHit.collider.tag == "SwingingDoor" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f))
				{
					raycastHit.collider.gameObject.GetComponent<SwingingDoorScript>().LockDoor(15f);
					ResetItem();
				}
			}
			else if (it == 3)
			{
				if (Physics.Raycast(ray, out raycastHit) && (raycastHit.collider.tag == "Door" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f))
				{
					DoorScript component = raycastHit.collider.gameObject.GetComponent<DoorScript>();
					if (component.DoorLocked)
					{
						component.UnlockDoor();
						component.OpenDoor();
						ResetItem();
					}
				}
			}
			else if (it == 4)
			{
				Instantiate<GameObject>(bsodaSpray, playerTransform.position, cameraTransform.rotation);
				ResetItem();
				player.ResetGuilt("drink", 1f);
				audioDevice.PlayOneShot(sfx[0]);
			}
			else if (it == 5)
			{
				if (Physics.Raycast(ray, out raycastHit))
				{
					if (raycastHit.collider.name == "BSODAMachine" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f)
					{
						ResetItem();
						CollectItem(4);
					}
					else if (raycastHit.collider.name == "ZestyMachine" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f)
					{
						ResetItem();
						CollectItem(1);
					}
					else if (raycastHit.collider.name == "PayPhone" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f)
					{
						raycastHit.collider.gameObject.GetComponent<TapePlayerScript>().Play();
						ResetItem();
					}
				}
			}
			else if (it == 6)
			{
				if (Physics.Raycast(ray, out raycastHit) && (raycastHit.collider.name == "TapePlayer" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f))
				{
					raycastHit.collider.gameObject.GetComponent<TapePlayerScript>().Play();
					ResetItem();
				}
			}
			else if (it == 7)
			{
				GameObject gameObject = Instantiate<GameObject>(alarmClock, playerTransform.position, cameraTransform.rotation);
				gameObject.GetComponent<AlarmClockScript>().baldi = baldiScrpt;
				ResetItem();
			}
			else if (it == 8)
			{
				if (Physics.Raycast(ray, out raycastHit) && (raycastHit.collider.tag == "Door" & Vector3.Distance(playerTransform.position, raycastHit.transform.position) <= 10f))
				{
					raycastHit.collider.gameObject.GetComponent<DoorScript>().SilenceDoor();
					ResetItem();
					audioDevice.PlayOneShot(sfx[1]);
				}
			}
			else if (it == 9)
			{
				if (player.jumpRope)
				{
					player.DeactivateJumpRope();
					playtimeScript.Disappoint();
					ResetItem();
				}
				else if (Physics.Raycast(ray, out raycastHit) && raycastHit.collider.name == "1st Prize")
				{
					firstPrizeScript.GoCrazy();
					ResetItem();
				}
			}
			else if (it == 10)
			{
				player.ActivateBoots();
				StartCoroutine(BootAnimation());
				ResetItem();
			}
			else if (it == 11)
			{
				StartCoroutine(Teleporter());
				ResetItem();
			}
		}
	}
	private IEnumerator BootAnimation()
	{
		float time = 15f;
		float height = 375f;
		Vector3 position = default(Vector3);
		boots.gameObject.SetActive(true);
		while (height > -375f)
		{
			height -= 375f * Time.deltaTime;
			time -= Time.deltaTime;
			position = boots.localPosition;
			position.y = height;
			boots.localPosition = position;
			yield return null;
		}
		position = boots.localPosition;
		position.y = -375f;
		boots.localPosition = position;
		boots.gameObject.SetActive(false);
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		boots.gameObject.SetActive(true);
		while (height < 375f)
		{
			height += 375f * Time.deltaTime;
			position = boots.localPosition;
			position.y = height;
			boots.localPosition = position;
			yield return null;
		}
		position = boots.localPosition;
		position.y = 375f;
		boots.localPosition = position;
		boots.gameObject.SetActive(false);
		yield break;
	}
	private IEnumerator Teleporter()
	{
		playerCharacter.enabled = false;
		playerCollider.enabled = false;
		int teleports = Random.Range(12, 16);
		int teleportCount = 0;
		float baseTime = 0.2f;
		float currentTime = baseTime;
		float increaseFactor = 1.1f;
		while (teleportCount < teleports)
		{
			currentTime -= Time.deltaTime;
			if (currentTime < 0f)
			{
				Teleport();
				teleportCount++;
				baseTime *= increaseFactor;
				currentTime = baseTime;
			}
			if (flipped)
			{
				player.height = 6f;
			}
			else
			{
				player.height = 4f;
			}
			yield return null;
		}
		playerCharacter.enabled = true;
		playerCollider.enabled = true;
		yield break;
	}
	private void Teleport()
	{
		AILocationSelector.GetNewTarget();
		player.transform.position = AILocationSelector.transform.position + Vector3.up * player.height;
		audioDevice.PlayOneShot(sfx[5]);
	}
	private void ResetItem()
	{
		item[itemSelected] = 0;
		itemSlot[itemSelected].texture = itemTextures[0];
		UpdateItemName();
	}
	public void LoseItem(int id)
	{
		item[id] = 0;
		itemSlot[id].texture = itemTextures[0];
		UpdateItemName();
	}
	private void UpdateItemName()
	{
		itemText.text = itemNames[item[itemSelected]];
	}
	public void ExitReached()
	{
		exitsReached++;
		if (exitsReached == 1)
		{
			audioDevice.volume = 0.8f;
			RenderSettings.ambientLight = Color.red; //Make everything red
			//RenderSettings.fog = true;
			audioDevice.PlayOneShot(sfx[4]);
			audioDevice.clip = CRAZYESCAPE[0];
			audioDevice.loop = true;
		}
		if (exitsReached == 2) //Play a sound
		{
			audioDevice.clip = CRAZYESCAPE[1];
		}
		if (exitsReached == 3) //Play a even louder sound
		{
			audioDevice.clip = CRAZYESCAPE[2];
			audioDevice.loop = false;
		}
		audioDevice.Play();
	}
	public void Fliparoo()
	{
		flipped = true;
		player.height = 6f;
		player.fliparoo = 180f;
		player.flipaturn = -1f;
		Camera.main.GetComponent<CameraScript>().offset = new Vector3(0f, -1f, 0f);
	}
	public CursorControllerScript cursorController;
	public PlayerScript player;
	public Transform playerTransform;
	public CharacterController playerCharacter;
	public Collider playerCollider;
	public AILocationSelectorScript AILocationSelector;
	public Transform cameraTransform;
	public Camera playerCamera;
    private int cullingMask;
	public EntranceScript[] entrances;
	public GameObject baldiTutor;
	public BaldiScript baldiScrpt;
	public AudioClip aud_Prize;
	public AudioClip aud_PrizeMobile;
	public AudioClip aud_AllNotebooks;
	private bool flipped;
	public GameObject[] spoopNpcs;
	public GameObject TestEnemy;
	public FirstPrizeScript firstPrizeScript;
	public PlaytimeScript playtimeScript;
	public GameObject quarter;
	public AudioSource tutorBaldi;
	public RectTransform boots;
	public string mode;
	public int notebooks;
	public int failedNotebooks;
	public bool spoopMode;
	public bool finaleMode;
	public bool debugMode;
	public bool mouseLocked;
	public int exitsReached;
	public int itemSelected;
	public int[] item = new int[3];
	public RawImage[] itemSlot = new RawImage[3];
	private string[] itemNames = new string[]
	{
		"Nothing",
		"Energy flavored Zesty Bar",
		"Yellow Door Lock",
		"Principal's Keys",
		"BSODA",
		"Quarter",
		"Baldi Anti Hearing and Disorienting Tape",
		"Alarm Clock",
		"WD-NoSquee (Door Type)",
		"Safety Scissors",
		"Big Ol' Boots",
		"Teleportation Teleporter"
	};
	public TMP_Text itemText;
	public Texture[] itemTextures = new Texture[10];
	public GameObject bsodaSpray;
	public GameObject alarmClock;
	public TMP_Text notebookCount;
	public GameObject pauseMenu;
	public string ExitGameScene;
	public GameObject highScoreText;
	public GameObject warning;
	public GameObject reticle;
	public RectTransform itemSelect;
	public int[] itemSelectOffset;
	public int itemSelectPosition;
	private bool gamePaused;
	private bool learningActive;
	private float gameOverDelay;
	public string GameOverScene;
	private AudioSource audioDevice;
	public AudioClip[] sfx;
	public AudioClip[] CRAZYESCAPE;
	public AudioSource schoolMusic;
	public AudioSource learnMusic;
	public TMP_Text popup;
	public float ok;
}
