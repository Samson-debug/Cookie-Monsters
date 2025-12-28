using UnityEngine;
using UnityEngine.UI;

namespace CookieGame.UI
{
    /// <summary>
    /// Loading screen with progress bar
    /// </summary>
    public class LoadingScreen : UIScreen
    {
        [Header("Progress")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _loadingTipText;

        [Header("Loading Tips")]
        [SerializeField] private string[] _loadingTips = new string[]
        {
            "Tip: Drag cookies to monsters to distribute them!",
            "Tip: Each monster gets the same number of cookies!",
            "Tip: Practice mode has no time limit!",
            "Tip: Answer quickly for bonus points!",
            "Tip: Division means sharing equally!"
        };

        protected override void OnShow()
        {
            base.OnShow();

            // Show random tip
            if (_loadingTipText != null && _loadingTips.Length > 0)
            {
                _loadingTipText.text = _loadingTips[Random.Range(0, _loadingTips.Length)];
            }

            UpdateProgress(0f);
        }

        /// <summary>
        /// Updates loading progress (0-1)
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }
    }
}
