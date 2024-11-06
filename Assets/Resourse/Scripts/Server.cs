using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    [Header("����� � ����")]
    public Unit playerUnit; // �����
    public Unit enemyUnit; // ����

    [Header("UI � ����")]
    public GameObject PlayerBlockUI; // UI ��� ���������� ����� ������
    public bool PlayerHod; // ����, �����������, ��� ��� (������ ��� �����)

    [Header("����� � �������")]
    public List<BafAndDebaf> _BafAndDebaf; // ������ ���� ������ � ��������
    [SerializeField] private GameObject baffPrefab; // ������ ��� �������� ������ � ��������

    [Header("���������� ���������� ����")]
    private bool cleansingActive; // ������� �� ������ ��������
    [SerializeField] private int currentTurn = 1; // ������� ��� � ����

    private void Awake()
    {
        // ��������� ������� ������� ����
        StartCoroutine(GameLoop());
    }
    private IEnumerator GameLoop()
    {
        // ������� ������� ����
        while (playerUnit.isAlive && enemyUnit.isAlive)
        {
            if (currentTurn % 2 == 0) // ��� ������
            {
                cleansingActive = false; // ��������� ��������
                yield return new WaitForSeconds(1.5f); // �������� ����� ����� �����
                Debug.Log("��� �����");

                // ����� ����������� �����
                int randomAbilityIndex = Random.Range(0, enemyUnit.abilities.Count);
                while (enemyUnit.abilities[randomAbilityIndex].CooldownTime > 0)
                {
                    randomAbilityIndex = Random.Range(0, enemyUnit.abilities.Count);
                    yield return new WaitForSeconds(0.1f);
                }

                // ���������� ����������� �����
                SelectAbility(randomAbilityIndex, enemyUnit, playerUnit);
                currentTurn++;
                PlayerHod = true; // ���������� ��� ������

                playerUnit.UpdateHealthBar(); // ��������� �������� ������
                enemyUnit.UpdateHealthBar(); // ��������� �������� �����
            }
            else // ��� �����
            {
                cleansingActive = false; // ��������� ��������
                Debug.Log("��� ������");
                PlayerBlockUI.SetActive(false); // ������������ UI

                // �������, ���� ����� ��������� ��������
                while (PlayerHod)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // ������� � ���������� ����
                currentTurn++;
                PlayerBlockUI.SetActive(true); // ��������� UI ��� ������
                playerUnit.UpdateHealthBar(); // ��������� �������� ������
                enemyUnit.UpdateHealthBar(); // ��������� �������� �����
            }
        }

        // ������ ��������� ����
        Debug.Log(playerUnit.isAlive ? "Win" : "Game Over");
        yield return new WaitForSeconds(1f);
        ResetGame();
        yield return null;
    }

    public void SelectAbility(int index, Unit caster, Unit target)
    {
        // ��������� ���������� ������� � ���������� �����������
        if (index >= 0 && index < caster.abilities.Count)
        {
            UseAbility(caster.abilities[index], caster, target);
            PlayerHod = false; // ��� ������ ��������
        }
    }

    private IEnumerator Cooldown(Ability ability)
    {
        // ���������� �������� ����������� �����������
        int cooldownEndTurn = currentTurn + ability.cooldown + 1;

        while (cooldownEndTurn > currentTurn)
        {
            yield return new WaitForSeconds(0.1f);
            ability.CooldownTime = cooldownEndTurn - currentTurn; // ��������� ����� �� ��������� �����������
        }

        yield return null;
    }

    private IEnumerator LongPower(Ability ability, Unit target, GameObject buff)
    {
        // ���������� �������� �����������
        int durationEndTurn = currentTurn + ability.duration + 1;
        int lastTurn = currentTurn;

        // ������������� ����� ������������ �������
        buff.GetComponent<Baff>().textDuration.text = ability.duration.ToString();

        while (durationEndTurn > currentTurn)
        {
            // ��������� �� ������� ��������
            if (cleansingActive && ability.name == "FireBall")
            {
                Debug.Log("�������� LongPower ��������� ����");
                cleansingActive = false; // �������� ���������
                StartCoroutine(EndBuffAndDebuff(ability.name, target)); // ��������� ������
                yield break; // ��������� ���������� ��������
            }

            yield return new WaitForSeconds(0.1f);
            if (lastTurn < currentTurn)
            {
                // ������� ���� ����, ���� ��� �� ������
                if (ability.name != "Barrier")
                {
                    target.TakeDamage(ability.LongPower);
                    if (ability.FVXPrefab != null)
                        Instantiate(ability.FVXPrefab, target.transform.position, Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }

                lastTurn = currentTurn; // ��������� ��������� ���

                if (buff != null)
                {
                    // ��������� ����� ������������ �������
                    buff.GetComponent<Baff>().textDuration.text = (durationEndTurn - lastTurn).ToString();
                }
                else
                {
                    StartCoroutine(EndBuffAndDebuff(ability.name, target)); // ��������� ������
                    yield break; // ��������� ���������� ��������
                }
            }
        }

        // ��������� �������� �����
        StartCoroutine(EndBuffAndDebuff(ability.name, target));
        yield return null;
    }

    private IEnumerator EndBuffAndDebuff(string name, Unit target)
    {
        // ���������� �������� ����� ��� �������
        bool isEnded = false;
        int completedChecks = 0;

        while (!isEnded)
        {
            // ������� �������
            for (int i = 0; i < target.debuffs.Count; i++)
            {
                if (target.debuffs[i].name == name)
                {
                    Destroy(target.debuffs[i]); // ������� ������
                    isEnded = true; // ������ ��������
                }
                if (i == target.debuffs.Count - 1) completedChecks++; // ���������, ���������� �� ��� �������
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.1f);

            // ������� �����
            for (int i = 0; i < target.buffs.Count; i++)
            {
                if (target.buffs[i].name == name)
                {
                    Destroy(target.buffs[i]); // ������� ����
                    isEnded = true; // ���� ��������
                }
                yield return new WaitForSeconds(0.1f);
                if (i == target.buffs.Count - 1) completedChecks++; // ���������, ���������� �� ��� �����
            }

            // ���������, ��������� �� ��� ��������
            isEnded = completedChecks == 2;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    private void UseAbility(Ability ability, Unit caster, Unit target)
    {
        // ������������� �����������
        if (ability.CooldownTime > 0)
        {
            Debug.Log("����������� � �����������!"); // ����������� �� ��
            return; // �����, ���� ����������� �� �����������
        }

        switch (ability.name)
        {
            case "Attack":
                // �����
                Debug.Log("�����");
                caster.animator.SetTrigger("Attack"); // ��������� �������� �����
                target.TakeDamage(ability.Power); // ������� ����
                ability.CooldownTime = ability.cooldown; // ������������� ����� ��
                StartCoroutine(Cooldown(ability)); // ��������� �������� ��� ������������ �����������
                break;

            case "Barrier":
                // �������� �������
                Debug.Log("������");
                Destroy(caster.buffs.Find(buff => buff.gameObject.name == "Barrier")); // ������� ������������ ������
                GameObject barrierBuff = Instantiate(baffPrefab, caster.buffPanel); // ������� ����� ����
                InitializeBuff(ability, caster, barrierBuff); // �������������� ����
                StartCoroutine(LongPower(ability, caster, barrierBuff)); // ��������� ���������� ��������


                if (ability.FVXPrefab != null)
                    barrierBuff.GetComponent<Baff>().fxEffect = Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity);


                caster.ApplyBarrier(ability.Power); // ��������� ������ �������
                ability.CooldownTime = ability.cooldown; // ������������� ����� ��
                StartCoroutine(Cooldown(ability)); // ��������� ��������
                break;

            case "Regeneration":
                // �����������
                Debug.Log("�����������");
                caster.TakeDamage(ability.Power); // ������� ����
                GameObject regenBuff = Instantiate(baffPrefab, caster.buffPanel); // ������� ����� ����
                InitializeBuff(ability, caster, regenBuff); // �������������� ����
                StartCoroutine(LongPower(ability, caster, regenBuff)); // ��������� ���������� ��������

                if (ability.FVXPrefab != null)
                    Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity); // ������� ������ ������������

                ability.CooldownTime = ability.cooldown; // ������������� ����� ��
                StartCoroutine(Cooldown(ability)); // ��������� ��������
                break;

            case "FireBall":
                // �������� ���
                Debug.Log("�������� ���");
                GameObject fireballDebuff = Instantiate(baffPrefab, target.buffPanel); // ������� ����� ������
                InitializeDebuff(ability, target, fireballDebuff); // �������������� ������

                target.TakeDamage(ability.Power + ability.LongPower); // ������� ����
                StartCoroutine(LongPower(ability, target, fireballDebuff)); // ��������� ���������� ��������

                ability.CooldownTime = ability.cooldown; // ������������� ����� ��
                StartCoroutine(Cooldown(ability)); // ��������� ��������
                break;

            case "Cleansing":
                // ��������
                Debug.Log("��������");
                cleansingActive = true; // ���������� ��������
                ability.CooldownTime = ability.cooldown; // ������������� ����� ��

                if (ability.FVXPrefab != null)
                    Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity); // ������� ������ ������������

                StartCoroutine(EndBuffAndDebuff("FireBall", caster)); // ������� �������
                StartCoroutine(Cooldown(ability)); // ��������� ��������
                break;
        }
    }

    private void InitializeBuff(Ability ability, Unit caster, GameObject buff)
    {
        // ������������� �����
        BafAndDebaf buffData = _BafAndDebaf.Find(b => b.Name == ability.name);
        if (buffData.Sprite != null)
            buff.GetComponent<Image>().sprite = buffData.Sprite; // ������������� ������

        buff.GetComponent<Image>().color = buffData.Color; // ������������� ����
        buff.name = buffData.name; // ������������� ���
        buff.GetComponent<Baff>().servak = this; // ������ �� ������
        caster.buffs.Add(buff); // ��������� ���� � ������
    }

    private void InitializeDebuff(Ability ability, Unit target, GameObject debuff)
    {
        // ������������� �������
        BafAndDebaf debuffData = _BafAndDebaf.Find(b => b.Name == ability.name);
        if (debuffData.Sprite != null)
            debuff.GetComponent<Image>().sprite = debuffData.Sprite; // ������������� ������

        debuff.GetComponent<Image>().color = debuffData.Color; // ������������� ����
        debuff.name = debuffData.name; // ������������� ���
        debuff.GetComponent<Baff>().servak = this; // ������ �� ������
        target.debuffs.Add(debuff); // ��������� ������ � �����
    }

    public void ResetGame()
    {
        // ����� ����
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ������������� ������� �����
    }
}
