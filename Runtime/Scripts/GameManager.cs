using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] List<GameObject> _objects;
   private AlphabetTracingEntryPoint alphabetTracingEntryPoint;
   [FormerlySerializedAs("Button")] [SerializeField] private Button ExitButton;
    private static int index=0;

    private void Awake()
    {
        ExitButton.onClick.AddListener(FinishGame);
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Change()
    {
        _objects[index].SetActive(false);
        index++;
        if (index<_objects.Count)
        {
            _objects[index].SetActive(true);
        }
        else
        {
            index = 0;
            SceneManager.LoadScene(0);
        }
    }

    public void ChangeObject()
    {
        StartCoroutine(WaitForNext());
        Change();
    }

    private IEnumerator WaitForNext()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private void OnApplicationQuit()
    {
        index = 0;
    }

    public void SetEntryPoint(AlphabetTracingEntryPoint entryPoint)
    {
        alphabetTracingEntryPoint = entryPoint;
    }

    private void SetFinishForPackage()
    {
        StartCoroutine(FinishAfterFireWorks());
    }

    private void FinishGame()
    {
        alphabetTracingEntryPoint.InvokeGameFinished();
       
    }

    private IEnumerator FinishAfterFireWorks()
    {
        yield return new WaitForSecondsRealtime(5f);
        alphabetTracingEntryPoint.InvokeGameFinished();
    }
}
