using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ����� ��� ���������� ������������ ������ ������� ������
public class SkillButtons : MonoBehaviour
{
    [SerializeField] private List<ButtonSkills> buttons; // ������ ������ �������
    [SerializeField] private List<TextMeshProUGUI> textCooldowns; // ������ ������� ��� ����������� ������� �����������
    [SerializeField] private Server server; // ������ �� ������ ��� �������������� � ������� � �����������

    // �����, ���������� ��� ������ ����
    void Start()
    {
        // ����� ����� ���������������� ��������� ���������, ���� ��� ����������
    }

    // �����, ���������� ������ ����
    void Update()
    {
        UpdateSkillButtons(); // ���������� ��������� ������ �������
    }

    // ���������� ��������� ������ ������� �� ������ ��������� �������� �����������
    private void UpdateSkillButtons()
    {
        for (int i = 0; i < server.playerUnit.abilities.Count; i++)
        {
            float cooldownTime = server.playerUnit.abilities[i].CooldownTime; // �������� ������� ����� �����������
            float maxCooldown = server.playerUnit.abilities[i].cooldown; // �������� ������������ ����� �����������

            // ���� ����������� ��� ��� �������
            if (cooldownTime > 0)
            {
                // ��������� ���������� ������ � ����� �����������
                buttons[i].ButtonImage.fillAmount = 1f - (cooldownTime / maxCooldown);
                textCooldowns[i].text = cooldownTime.ToString("0"); // ����������� ����� � ������
            }
            else // ���� ����������� ���������
            {
                buttons[i].ButtonImage.fillAmount = 1f; // ��������� ������ ���������
                textCooldowns[i].text = "0"; // ���������� ����� �������
            }
        }
    }

    // ����� ��� ��������� ������ ������
    public void ActivatePlayerSkill(int skillIndex)
    {
        // ����� ������ � ���������� ��� � ����������
        server.SelectAbility(skillIndex, server.playerUnit, server.enemyUnit);
    }
}

// ����� ��� �������� ������ ������
[System.Serializable]
public class ButtonSkills
{
    public string skillName; // �������� ������
    public Image ButtonImage; // ����������� ������
    public int indexSkill; // ������ ������ ��� �������������
}
