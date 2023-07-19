using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    GameManager gameManager;
    public Vector3 targetTileLocation;
    private int targetTile = -1;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision){
        if (collision.gameObject.CompareTag("Ship"))
        {
            if (collision.gameObject.name == "Submarine") targetTileLocation.y += 0.3f;
            gameManager.EnemyHitPlayer(targetTileLocation, true, collision.gameObject);
        } else{
            gameManager.EnemyHitPlayer(targetTileLocation, false, collision.gameObject);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collider){
        targetTileLocation = collider.gameObject.transform.position;
    }

    public void SetTarget(int target){
        targetTile = target;
    }
}
