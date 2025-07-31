using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : UIGameObject
{
    public Image currentHealthBar;
    public Image healthLossBar;
    public float healthLossBarTime = 1f;
    public bool destroyOnEmpty = false;


    private void Update()
    {
        if(destroyOnEmpty && currentHealthBar.fillAmount == 0 && healthLossBar.fillAmount == 0)
        {
            Destroy(gameObject, 1f);
        }
    }

    public void UpdateFillAmount(float current, float max)
    {
        currentHealthBar.fillAmount = current / max;
        StartCoroutine(UpdateHealthLossBar());
    }

    IEnumerator UpdateHealthLossBar()
    {
        yield return new WaitForSeconds(healthLossBarTime);

        while (healthLossBar.fillAmount > currentHealthBar.fillAmount)
        {
            healthLossBar.fillAmount -= Time.deltaTime;
            yield return null;
        }

        healthLossBar.fillAmount = currentHealthBar.fillAmount;
    }
}
