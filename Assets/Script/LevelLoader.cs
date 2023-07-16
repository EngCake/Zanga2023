using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CakeEngineering
{
    public class LevelLoader : MonoBehaviour
    {
        public UnityEvent OnLevelLoad;

        private void Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            LeanTween.color(rectTransform, Color.clear, 1.0f)
                .setOnComplete(OnLevelLoad.Invoke);
        }

        public void TransitionToNextScene()
        {
            var rectTransform = GetComponent<RectTransform>();
            LeanTween.color(rectTransform, Color.black, 1.0f)
                .setOnComplete(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
        }
    }
}