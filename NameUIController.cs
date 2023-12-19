using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NameUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public string driverName = "";
    public TextMeshProUGUI playerName;
    //public Text playerName;
    public Transform target;
    CanvasGroup canvasGroup;
    public Renderer carRend;
    public TextMeshProUGUI lapDisplay;
    CheckpointManage  cpManager;
    int carRego;


    bool regoSet = false;
    void Start()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        playerName = this.GetComponentInChildren<TextMeshProUGUI>();  // <- 수정
        canvasGroup = this.GetComponent<CanvasGroup>();
        
        //lapDisplay = transform.Find("Lap").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!racestarter.racing){canvasGroup.alpha = 0; return;}
        if(!regoSet)
        {
            carRego = Leaderboard.RegisterCar(driverName);
            regoSet = true;
            return;
        }
        
        if(carRend == null) return;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool carInView = GeometryUtility.TestPlanesAABB(planes, carRend.bounds);
        canvasGroup.alpha = carInView ? 1:0;

        this.transform.position = Camera.main.WorldToScreenPoint(target.position+Vector3.up*1.2f);
        playerName.text = driverName;
        if(cpManager == null)
            cpManager = target.GetComponent<CheckpointManage>();
        Leaderboard.SetPosition(carRego, cpManager.lap, cpManager.checkpoint, cpManager.timeEntered);
        string position = Leaderboard.GetPosition(carRego);

        lapDisplay.text = position + " " ;




    }
}
