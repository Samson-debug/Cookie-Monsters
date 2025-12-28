using TMPro;
using UnityEngine;

namespace CookieGame.Gameplay
{
    public class QuestionInputField : MonoBehaviour
    {
        public TMP_InputField dividendTMP;
        public TMP_InputField divisorTMP;

        private void Start()
        {
            dividendTMP.onEndEdit.AddListener((string value) =>
            {
                if (int.TryParse(value, out int dividend))
                {
                    dividendTMP.text = Mathf.Clamp(dividend, 5, 20).ToString();
                }
            });
            divisorTMP.onEndEdit.AddListener((string value) =>
            {
                if (int.TryParse(value, out int divisor))
                {
                    divisorTMP.text = Mathf.Clamp(divisor, 1, 5).ToString();
                }
            });
        }

        public (int dividend, int divisor) GetQuestionData()
        {
            int dividend = 5;
            int divisor = 1;

            if (!int.TryParse(dividendTMP.text, out dividend))
                dividend = 5;

            if (!int.TryParse(divisorTMP.text, out divisor) || divisor == 0)
                divisor = 1;

            return (dividend, divisor);
        }
    }
}