using UnityEngine;

public class PlayerBranchProgression : MonoBehaviour
{
    private OathDamageShield oathDamageShield;
    private OathExpHeal oathExpHeal;
    private OathPurificationPulse oathPurificationPulse;
    private FallenKillLeech fallenKillLeech;
    private FallenLowHealthFrenzy fallenLowHealthFrenzy;
    private FallenDeathBurst fallenDeathBurst;

    public void UnlockOathDamageShield()
    {
        if (oathDamageShield == null)
        {
            oathDamageShield = GetComponent<OathDamageShield>();
            if (oathDamageShield == null)
            {
                oathDamageShield = gameObject.AddComponent<OathDamageShield>();
            }
        }

        oathDamageShield.AddLevel();
    }

    public void UnlockOathExpHeal()
    {
        if (oathExpHeal == null)
        {
            oathExpHeal = GetComponent<OathExpHeal>();
            if (oathExpHeal == null)
            {
                oathExpHeal = gameObject.AddComponent<OathExpHeal>();
            }
        }

        oathExpHeal.AddLevel();
    }

    public void UnlockOathPurificationPulse()
    {
        if (oathPurificationPulse == null)
        {
            oathPurificationPulse = GetComponent<OathPurificationPulse>();
            if (oathPurificationPulse == null)
            {
                oathPurificationPulse = gameObject.AddComponent<OathPurificationPulse>();
            }
        }

        oathPurificationPulse.AddLevel();
    }

    public void UnlockFallenKillLeech()
    {
        if (fallenKillLeech == null)
        {
            fallenKillLeech = GetComponent<FallenKillLeech>();
            if (fallenKillLeech == null)
            {
                fallenKillLeech = gameObject.AddComponent<FallenKillLeech>();
            }
        }

        fallenKillLeech.AddLevel();
    }

    public void UnlockFallenLowHealthFrenzy()
    {
        if (fallenLowHealthFrenzy == null)
        {
            fallenLowHealthFrenzy = GetComponent<FallenLowHealthFrenzy>();
            if (fallenLowHealthFrenzy == null)
            {
                fallenLowHealthFrenzy = gameObject.AddComponent<FallenLowHealthFrenzy>();
            }
        }

        fallenLowHealthFrenzy.AddLevel();
    }

    public void UnlockFallenDeathBurst()
    {
        if (fallenDeathBurst == null)
        {
            fallenDeathBurst = GetComponent<FallenDeathBurst>();
            if (fallenDeathBurst == null)
            {
                fallenDeathBurst = gameObject.AddComponent<FallenDeathBurst>();
            }
        }

        fallenDeathBurst.AddLevel();
    }
}
