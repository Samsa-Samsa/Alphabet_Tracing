using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Filler : MonoBehaviour
{
    private bool _shouldMove = true;

    [SerializeField] Transform _eraser;
    private Vector3 targetDire;


    [SerializeField] Transform _pivot;
    [SerializeField] Transform _startPosition;
    private Vector3 lastMouseDirection;

    private Quaternion _initialRotation;
    private Quaternion _initialMouseFollowerRoation;


    [SerializeField] GameObject nextObject;

    [SerializeField] Transform cursor;
    [SerializeField] Transform destination;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D spriteCol;

    [SerializeField] float startThreshold;
    [SerializeField] float endThreshold = 0.35f;

    [SerializeField] float zOffset=10;
    //Vector3 startPosition;

    RenderTexture renderTex;
    [SerializeField] Camera maskCamera;

    MaterialPropertyBlock matPropBlock;

    [SerializeField] Texture2D arrowText;

    [SerializeField] GameObject _mouseFollower;

    private static int counter = 0;

    [SerializeField] float cameraSize = 5;
    [SerializeField] LayerMask drawLayer;

    [SerializeField] GameObject maskReset;

    void Start()
    {
         targetDire = (_eraser.position - cursor.position).normalized;
        _initialRotation = cursor.rotation;
        _initialMouseFollowerRoation = _mouseFollower.transform.rotation;
        _shouldMove = true;
        _mouseFollower.transform.position += targetDire * 0.45f;


        if (maskCamera == null)
        {
            maskCamera = new GameObject("maskCamera").AddComponent<Camera>();
            maskCamera.transform.position = transform.position + new Vector3(0, 0, -10);
            maskCamera.transform.parent = transform;

            maskCamera.clearFlags = CameraClearFlags.Nothing;
            maskCamera.orthographic = true;
            maskCamera.orthographicSize = spriteRenderer.bounds.size.y;
            maskCamera.cullingMask = drawLayer;
        }

        renderTex = new RenderTexture(Screen.width / 2, Screen.height / 2, 16, RenderTextureFormat.ARGB32);
        maskCamera.targetTexture = renderTex;

        Shader.SetGlobalTexture("_LetterMaskTex", renderTex);

        matPropBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(matPropBlock);

        matPropBlock.SetTexture("_EmptyTex", arrowText);

        spriteRenderer.SetPropertyBlock(matPropBlock);

    }

    

    private void Update()
    {
        targetDire = (_eraser.position - cursor.position).normalized;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = touch.position;
            touchPos.z = zOffset;

            if (Vector3.Distance(Camera.main.ScreenToWorldPoint(touchPos), cursor.position) < startThreshold&& _shouldMove)
            {
                if (!cursor.gameObject.activeInHierarchy)
                {
                    cursor.gameObject.SetActive(true);
                }
                /*if (!_mouseFollower.gameObject.activeInHierarchy)
                {
                    _mouseFollower.SetActive(true);
                }*/

                cursor.position = Camera.main.ScreenToWorldPoint(touchPos);
                
                _mouseFollower.transform.position = cursor.position;

                if (_pivot != null)
                {
                    Vector3 currentDirection = (cursor.position - _pivot.position).normalized;

                    if (lastMouseDirection == Vector3.zero)
                    {
                        lastMouseDirection = currentDirection;
                    }

                    float angle = Vector3.SignedAngle(lastMouseDirection, currentDirection, Vector3.forward);
                    
                    cursor.RotateAround(_pivot.position, Vector3.forward, angle);


                    //Vector3 targetDire=(_eraser.position-cursor.position).normalized;
                    lastMouseDirection = currentDirection;
                    _mouseFollower.transform.position += targetDire * 0.45f;
                    _mouseFollower.transform.RotateAround(_pivot.position, Vector3.forward, angle);
                }

                if (Vector3.Distance(cursor.position, destination.position) < endThreshold)
                {
                    FinishFilling();
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                ResetCursor();
                _shouldMove = true;
            }
            
        }
        else
        {
            ResetCursor();
        }
    }

    void OnMouseExit()
    {
        _shouldMove = false;
        ResetCursor();
    }

    void ResetCursor()
    {
        cursor.position = _startPosition.position;
        lastMouseDirection = Vector3.zero;
        cursor.rotation = _initialRotation;

        StartCoroutine(ResetRenderTex(false));
    }

    IEnumerator ResetRenderTex(bool full)
    {
        maskReset.SetActive(true);
        yield return new WaitForEndOfFrame();
        maskReset.SetActive(false);
        cursor.gameObject.SetActive(false);
        _mouseFollower.transform.position = _startPosition.position;
        _mouseFollower.transform.rotation = _initialMouseFollowerRoation;



        if (full)
        {
            Destroy(spriteCol);
            Destroy(cursor.gameObject);
            Destroy(destination.gameObject);
            if(nextObject != null) 
                nextObject.SetActive(true);

            //Destroy(renderTex);

            _mouseFollower.gameObject.SetActive(false);

            counter++;

            if (counter == 5)
            {
                GameManager.Instance.ChangeObject();
            }
            else if (counter == 10)
            {
                GameManager.Instance.ChangeObject();
            }
            else if (counter == 12)
            {
                GameManager.Instance.ChangeObject();
                counter = 0;
            }
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        counter = 0;
    }

    void FinishFilling()
    {
        spriteRenderer.GetPropertyBlock(matPropBlock);
        matPropBlock.SetInt("_IsFull", 1);
        spriteRenderer.SetPropertyBlock(matPropBlock);

        StartCoroutine(ResetRenderTex(true));
    }
}
