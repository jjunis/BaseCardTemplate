using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] [Range(0, 5)] private float selectionSpacing = 1;
    [SerializeField] private Vector3 curveStart = new Vector3(2f, -0.7f, 0);
    [SerializeField] private Vector3 curveEnd = new Vector3(-2f, -0.7f, 0);
    [SerializeField] private Vector2 handOffset = new Vector2(0, -0.3f);
    [SerializeField] private Vector2 handSize = new Vector2(9, 1.7f);

    public List<GameObject> CardList = new List<GameObject>();

    [SerializeField] private int _selected = -1; // Card index that is nearest to mouse
    [SerializeField] private int _dragged = -1; // Card index that is held by mouse (inside of hand)

    private Camera _mainCam;

    [SerializeField] public Plane _plane; // world XY plane, used for mouse position raycasts
    private Vector3 _a, _b, _c; // Used for shaping hand into curve
   
    [SerializeField] private Vector3 _mouseWorldPos;
    [SerializeField] private Vector3 _heldCardOffset;

    public GameObject Card;

    bool isPlayed = false;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void Start()
    {
        InitHand();
    }

    private void Update()
    {      

        GetMouseWorldPosition(Input.mousePosition);
        CardPos();

        if(!isPlayed)
        {
            GetMouse();
        }
       

        if(Input.GetKeyDown(KeyCode.Space))
        {
            DrawCard();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayCard();
        }
      

    }

    private void DrawCard()
    {
        GameObject temp = Instantiate(Card);
       
        CardList.Add(temp);
        for(int i = 0; i < CardList.Count; i++)
        {
            CardList[i].GetComponent<CardBase>().HandCardIndex = i;
        }
    }

    private void PlayCard()
    {
        if(_dragged != -1 && _selected != -1)
        {
            GameObject temp = CardList[_dragged];
            CardList.RemoveAt(_dragged);
            Destroy(temp);
            for (int i = 0; i < CardList.Count; i++)
            {
                CardList[i].GetComponent<CardBase>().HandCardIndex = i;
            }
            isPlayed = false;
            _dragged = -1;
            _selected = -1;
        }
      
    }



    private void GetMouse()
    {
        RaycastHit hit;
        var mainRay = _mainCam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(mainRay, out hit, 1000))
        {
            if(hit.collider.gameObject.tag == "Card")
            {
                _selected = hit.collider.gameObject.GetComponent<CardBase>().HandCardIndex;

                if(Input.GetMouseButton(0))
                {
                    _dragged = hit.collider.gameObject.GetComponent<CardBase>().HandCardIndex;
                }
          
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if(_dragged >= 0)
            {
                if(hit.collider.gameObject.GetComponent<CardBase>().MouseOff() != null)
                {
                    PlayCard();
                }
            }
            _dragged = -1;
        }



    }

    private void CardPos()
    {

        for(int i  = 0; i < CardList.Count; i++)
        {
            float selectOffset = 0;
            var t = (i + 0.5f) / CardList.Count + selectOffset * selectionSpacing;
            var p = GetCurvePoint(_a, _b, _c, t);
            var cardTransform = CardList[i].transform;
            var cardForward = Vector3.forward;
            var cardUp = GetCurveNormal(_a, _b, _c, t);
            Vector3 cardPos = p + (cardTransform.up * 0.3f);
            cardTransform.rotation = Quaternion.RotateTowards(cardTransform.rotation,
                    Quaternion.LookRotation(cardForward, cardUp), 80f * Time.deltaTime);

            var mouseHoveringOnSelected = _selected == i;
            var onDraggedCard = _dragged == i;
           

            // Sorting Order
            if (mouseHoveringOnSelected)
            {               
                cardPos.z = transform.position.z - 0.2f;
            }
            else
            {
                cardPos.z = transform.position.z + t * 0.5f;
            }

            if (onDraggedCard)
            {
                cardPos = _mouseWorldPos;
                cardTransform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
                CardList[i].GetComponent<CardBase>().CardInDrag = true;
            }
            else
            {
                CardList[i].GetComponent<CardBase>().CardInDrag = false;
            }



            cardTransform.position = cardPos;

        }


    }

    private void GetMouseWorldPosition(Vector2 mousePos)
    {
        var ray = _mainCam.ScreenPointToRay(mousePos);
        if (_plane.Raycast(ray, out var enter)) _mouseWorldPos = ray.GetPoint(enter);
    }

    private void InitHand()
    {
        _a = transform.TransformPoint(curveStart);
        _b = transform.position;
        _c = transform.TransformPoint(curveEnd);

        _plane = new Plane(-Vector3.forward, transform.position);
        
    }


    public static Vector3 GetCurvePoint(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return (oneMinusT * oneMinusT * a) + (2f * oneMinusT * t * b) + (t * t * c);
    }

    public static Vector3 GetCurveNormal(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 tangent = GetCurveTangent(a, b, c, t);
        return Vector3.Cross(tangent, Vector3.forward);
    }

    public static Vector3 GetCurveTangent(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(curveStart, 0.03f);
        //Gizmos.DrawSphere(Vector3.zero, 0.03f);
        Gizmos.DrawSphere(curveEnd, 0.03f);

        Vector3 p1 = curveStart;
        for (int i = 0; i < 20; i++)
        {
            float t = (i + 1) / 20f;
            Vector3 p2 = GetCurvePoint(curveStart, Vector3.zero, curveEnd, t);
            Gizmos.DrawLine(p1, p2);
            p1 = p2;
        }

        Gizmos.DrawWireCube(handOffset, handSize);
    }
}
