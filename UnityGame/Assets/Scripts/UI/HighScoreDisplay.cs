using UnityEngine.UI;

public class HighScoreDisplay : UIelement
{
    public Text displayText = null;

    public void DisplayHighScore()
    {
        if (displayText != null)
        {
            displayText.text = "High: " + GameManager.instance.highScore.ToString();
        }
    }

    public override void UpdateUI()
    {
        // This calls the base update UI function from the UIelement class
        base.UpdateUI();

        // The remaining code is only called for this sub-class of UIelement and not others
        DisplayHighScore();
    }
}
