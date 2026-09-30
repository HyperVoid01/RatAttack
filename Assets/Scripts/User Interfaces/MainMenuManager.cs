using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject settingsUI;
    
    [Header("Camera Positions")]
    [SerializeField] private Transform mainMenuCameraPosition;
    [SerializeField] private Transform settingsCameraPosition;

    [Header("Camera Movement")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float panDuration;

    public void OnClickPlay()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ButtonClick, transform.position);
        SceneManager.LoadScene(1);
    }
    
    public void OnClickSettings()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ButtonClick, transform.position);
        
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(true);

        StartCoroutine(PanCamera(mainMenuCameraPosition.position, mainMenuCameraPosition.rotation,
            settingsCameraPosition.position, settingsCameraPosition.rotation, () =>
            {
                settingsUI.SetActive(true);
            }));
    }

    public void OnClickBack()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ButtonClick, transform.position);
        
        settingsUI.SetActive(false);
        
        StartCoroutine(PanCamera(settingsCameraPosition.position, settingsCameraPosition.rotation,
            mainMenuCameraPosition.position, mainMenuCameraPosition.rotation, () =>
            {
                mainMenuUI.SetActive(true);
            }));
    }

    public void OnClickExit()
    {
        SoundPlayer.Instance.PlaySound(SoundID.ButtonClick, transform.position);
        
        Application.Quit();
    }
    
    private IEnumerator PanCamera(Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        onComplete?.Invoke();
    }
}
