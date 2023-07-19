using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    GameManager gameManager;
    Ray ray;
    RaycastHit hit;
    public bool missileHit = false;
    public bool enemyMissileHit = false;
    Color32[] hitcolor = new Color32[2];
    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        hitcolor[0] = gameObject.GetComponent<MeshRenderer>().material.color;
        hitcolor[1] = gameObject.GetComponent<MeshRenderer>().material.color;
        originalColor = GetComponent<Renderer>().material.color;
    
    }

    private void OnTriggerEnter(Collider collider){
        if(collider.gameObject.CompareTag("Missile")){
            missileHit = true;
        } else{
            if(collider.gameObject.CompareTag("EnemyMissile")){
                enemyMissileHit = true;
                hitcolor[0] = new Color32(38, 57, 76, 255);
                GetComponent<Renderer>().material.color = hitcolor[0];
            }
        }
    }

    public void SetTileColor(int index, Color32 color){
        hitcolor[index] = color;
    }

    public void SwitchColors(int colorIndex){
        GetComponent<Renderer>().material.color = hitcolor[colorIndex];
    }

    public void FlashColor(Color tempColor){
        GetComponent<Renderer>().material.color = tempColor;
        Invoke("ResetColor",0.5f);
    }

    private void ResetColor(){
        GetComponent<Renderer>().material.color = originalColor;
    }
}
