using UnityEngine;
using TMPro;

public class UpgradeBox : Box
{
    [SerializeField] int price;
    [SerializeField] float increaseMult;

    [SerializeField] float upgradePercent;

    [SerializeField] TongueScript tongue;
    [SerializeField] MovementPlatformer2D movement;

    [SerializeField] TextMeshPro priceText;

    [SerializeField] UpgradeFunction function;

    string[] functionNames = { 
        "TongueLength",
        "TongueSpeed",
        "MovementSpeed",
        "JumpHeight",
        "TonguePullPower",
        "TractionIncrease",
        "BootySlide",
        "ThrowForce",
        "TimeIncrease",
        "Wealth",
    };

    Collider2D coll;

    enum UpgradeFunction
    {
        tongueLength,
        tongueSpeed,
        movementSpeed,
        jumpHeight,
        tonguePullPower,
        tractionUp,
        bootySlide,
        throwForce,
        timeIncrease,
        wealth
    }

    private void Start()
    {
        coll = GetComponent<Collider2D>();
        price = Mathf.RoundToInt(price * Mathf.Pow(increaseMult, SaveData.instance.playerStats.upgrades[(int)function]));
        priceText.text = "$" + price.ToString();
    }

    private void Update()
    {
        if (GameManager.instance.playerCash >= price)
        {
            coll.enabled = true;
        }
        else
        {
            coll.enabled = false;
            GetComponent<SpriteRenderer>().color = Color.gray;
            transform.Find("Icon").GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameManager.instance.playerCash -= price;
        SaveData.instance.playerStats.upgrades[(int)function]++;
        BroadcastMessage(functionNames[(int)function]);
        Destroy(gameObject);
    }

    void TongueLength()
    {
        Debug.Log("UPGRADED TONGUE LENGTH");
        tongue.M.tongueLength *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void TongueSpeed()
    {
        Debug.Log("UPGRADED TONGUE SPEED");
        tongue.M.tongueShootTime *= 1 - upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void MovementSpeed()
    {
        Debug.Log("UPGRADED MOVE SPEED");
        movement.M.movementSpeed *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void JumpHeight()
    {
        Debug.Log("UPGRADED JUMP HEIGHT");
        movement.M.jumpForce *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void TonguePullPower()
    {
        Debug.Log("UPGRADED TONGUE PULL");
        tongue.M.tonguePullForce *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void TractionIncrease()
    {
        Debug.Log("UPGRADED TRACTION");
        movement.M.decceleration *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void BootySlide()
    {
        Debug.Log("UPGRADED BOOTY SLIDE");
    }

    void ThrowForce()
    {
        Debug.Log("UPGRADED THROW FORCE");
        tongue.M.throwForce *= 1 + upgradePercent;
        SaveData.instance.UpdateStats();
    }

    void TimeIncrease()
    {
        Debug.Log("UPGRADED TIME INCREASE");
        GameManager.instance.phaseTimer++;
        SaveData.instance.UpdateStats();
    }

    void Wealth()
    {
        Debug.Log("UPGRADED WEALTH GAP");
        GameManager.instance.cashPerLabelInOrder = Mathf.RoundToInt(GameManager.instance.cashPerLabelInOrder * 1.25f);
        GameManager.instance.cashPerBonusOrder = Mathf.RoundToInt(GameManager.instance.cashPerBonusOrder * 1.5f);
        SaveData.instance.UpdateStats();
    }
}
