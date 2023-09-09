using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    private int shipIndex = 0;
    private Ship shipScript;
    public List<Tile> allTileScripts;


    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public Button hitBtn;
    public Button missBtn;
    public Button confirmBtn;
    public Button playerOneBtn;
    public Button playerTwoBtn;
    public Button closeBtn;
    public Button infoBtn;
    public GameObject infoPanel;
    public TMP_Text topText;
    public TMP_Text playerShipText;
    public TMP_Text enemyHitText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject firePrefab;
    public GameObject woodDock;

    private bool setupComplete = false;
    private bool playerTurn = true;
    private bool hitSelected = false;
    private bool canSelect = true;
    private bool gamePaused = false;

    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();

    private int enemyHitsLeft = 17;
    private int playerShipCount = 5;

    private Vector3 selectedTilePos;
    private PlayerInput playerInput;
    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    Ray ray;
    RaycastHit hit;


    // Start is called before the first frame update
    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<Ship>();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateClicked());
        replayBtn.onClick.AddListener(() => ReplayClicked());
        hitBtn.onClick.AddListener(() => HitClicked());
        missBtn.onClick.AddListener(() => MissClicked());
        confirmBtn.onClick.AddListener(() => ConfirmClicked());
        playerOneBtn.onClick.AddListener(() => PlayerOneClicked());
        playerTwoBtn.onClick.AddListener(() => PlayerTwoClicked());
        infoBtn.onClick.AddListener(() => InfoClicked());
        closeBtn.onClick.AddListener(() => CloseClicked());
    }

    private void Awake(){
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
    }

    private void OnEnable(){
        touchPressAction.performed += TouchPressed;
    }

    private void OnDisable(){
        touchPressAction.performed -= TouchPressed;
    }

    private void TouchPressed(InputAction.CallbackContext context){
        ray = Camera.main.ScreenPointToRay(touchPositionAction.ReadValue<Vector2>());
        if(Physics.Raycast(ray, out hit)){
            if(Input.GetMouseButtonDown(0) && hit.collider.gameObject.CompareTag("Tile")){
                if((hit.collider.gameObject.GetComponent<Tile>().missileHit == false && playerTurn)
                || (hit.collider.gameObject.GetComponent<Tile>().enemyMissileHit == false && !playerTurn)){
                    TileClicked(hit.collider.gameObject);
                }
            }
        }
    }

    private void NextShipClicked()
    {
        if(!shipScript.OnGameBoard()){
            shipScript.FlashColor(Color.red);
        } else{
            if(shipIndex <= ships.Length -2){
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<Ship>();
                shipScript.FlashColor(Color.yellow);
            } else{
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.gameObject.SetActive(false);
                canSelect = false;
                playerOneBtn.gameObject.SetActive(true);
                playerTwoBtn.gameObject.SetActive(true);
                topText.text = "Select your player number";
                setupComplete = true;
                for(int i = 0; i < ships.Length; i++){
                    ships[i].SetActive(false);
                }
            }
        }
        
    }

    public void TileClicked(GameObject tile){
        if(!gamePaused){
            if(setupComplete && canSelect){
                if(playerTurn){
                    tile.GetComponent<Tile>().FlashColor(Color.yellow);
                    selectedTilePos = tile.transform.position;
                    hitBtn.gameObject.SetActive(true);
                    missBtn.gameObject.SetActive(true);
                } else{
                    tile.GetComponent<Tile>().FlashColor(Color.yellow);
                    selectedTilePos = tile.transform.position;
                    confirmBtn.gameObject.SetActive(true);
                }
            } else{
                if(!setupComplete){
                    PlaceShip(tile);
                    shipScript.SetClicketTile(tile);
                }
            }
        }
    }

    private void PlaceShip(GameObject tile){
        shipScript = ships[shipIndex].GetComponent<Ship>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    void RotateClicked(){
        shipScript.RotateShip();
    }

    public void CheckHit(GameObject tile){
        if(hitSelected){
            topText.text = "HIT!";
            tile.GetComponent<Tile>().SetTileColor(1, new Color32(255, 0, 0, 255));
            tile.GetComponent<Tile>().SwitchColors(1);
            enemyHitsLeft--;
            enemyHitText.text = enemyHitsLeft.ToString();
            hitSelected = false;
        } else{
            tile.GetComponent<Tile>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<Tile>().SwitchColors(1);
            topText.text = "Missed";
        }
        Invoke("EndPlayerTurn", 2.0f);
    }

    public void EnemyHitPlayer(Vector3 tile, bool hit, GameObject hitObj){
        if(hit){
            topText.text = "HIT!";
            tile.y += 0.2f;
            playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));
            if (hitObj.GetComponent<Ship>().HitCheckSank()){
                playerShipCount--;
                playerShipText.text = playerShipCount.ToString();
            }
        } else{
            topText.text = "Missed";
        }
        Invoke("EndEnemyTurn", 2.0f);
    }

    private void EndPlayerTurn(){
        if(enemyHitsLeft < 1){
            GameOver("YOU WIN!");
        } else{
            for(int i = 0; i < ships.Length; i++){
                ships[i].SetActive(true);
            }
            foreach(GameObject fire in playerFires){
                fire.SetActive(true);
            } 
            foreach(GameObject fire in enemyFires){
                fire.SetActive(false);
            }
            topText.text = "Opponents's turn";
            canSelect = true;
            ColorAllTiles(0);
        }
    }

    private void EndEnemyTurn(){
        if(playerShipCount < 1){
            GameOver("OPPONENT WINS!");
        } else{
            for(int i = 0; i < ships.Length; i++){
                ships[i].SetActive(false);
            }
            foreach(GameObject fire in playerFires){
                fire.SetActive(false);
            } 
            foreach(GameObject fire in enemyFires){
                fire.SetActive(true);
            }
            topText.text = "Select a tile";
            playerTurn = true;
            canSelect = true;
            ColorAllTiles(1);
        }
    }

    private void ColorAllTiles(int colorIndex){
        foreach(Tile tileScript in allTileScripts){
            tileScript.SwitchColors(colorIndex);
        }
    }

    void GameOver(string winner){
        topText.text = "Game over: " + winner;
        replayBtn.gameObject.SetActive(true);
        canSelect = false;
    }
    void ReplayClicked(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void HitClicked(){
        canSelect = false;
        hitSelected = true;
        selectedTilePos.y += 15;
        playerTurn = false;
        Instantiate(missilePrefab, selectedTilePos, missilePrefab.transform.rotation);
        hitBtn.gameObject.SetActive(false);
        missBtn.gameObject.SetActive(false);
    }

    void MissClicked(){
        canSelect = false;
        hitSelected = false;
        selectedTilePos.y += 15;
        playerTurn = false;
        Instantiate(missilePrefab, selectedTilePos, missilePrefab.transform.rotation);
        hitBtn.gameObject.SetActive(false);
        missBtn.gameObject.SetActive(false);
    }

    void ConfirmClicked(){
        canSelect = false;
        selectedTilePos.y += 15;
        Instantiate(enemyMissilePrefab, selectedTilePos, enemyMissilePrefab.transform.rotation);
        confirmBtn.gameObject.SetActive(false);
    }

    void PlayerOneClicked(){
        topText.text = "Select a tile";
        canSelect = true;
        playerOneBtn.gameObject.SetActive(false);
        playerTwoBtn.gameObject.SetActive(false);
    }

    void PlayerTwoClicked(){
        playerTurn = false;
        EndPlayerTurn();
        canSelect = true;
        playerOneBtn.gameObject.SetActive(false);
        playerTwoBtn.gameObject.SetActive(false);
    }

    void InfoClicked(){
        gamePaused = true;
        infoPanel.SetActive(true);
    }

    void CloseClicked(){
        gamePaused = false;
        infoPanel.SetActive(false);
    }
}
