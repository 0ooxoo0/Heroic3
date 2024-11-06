using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [Header("�������� ��������")]
    public int health = 20;                // ������� �������� �����
    public int maxHealth = 20;             // ������������ �������� �����
    public bool isAlive = true;             // ����, �����������, ��� ���� ��� ���

    [Header("�������� UI")]
    public Slider healthBar;                // ������� ��������
    [SerializeField] private TextMeshProUGUI hpText; // ��������� ����������� ��������

    public Animator animator;                // �������� ��� ���������� ����������
    public List<Ability> abilities;         // ������ ������������ �����
    public int barrierHP;                   // �������� �������
    public Transform buffPanel;             // ������ ��� ����������� ������

    [Header("�������")]
    public List<GameObject> debuffs;       // ������ ��������
    public List<GameObject> buffs;          // ������ ������

    private void Start()
    {
        // �������� ��������� ��������� � ��������� ������ �������� ��� ������
        animator = GetComponent<Animator>();
        UpdateHealthBar();
    }

    // ����� ��� ��������� �����
    public void TakeDamage(int damage)
    {
        // ��������, ��� �� ����
        if (isAlive)
        {
            // ��������� ����� � ����������� �� ������� �������
            if (barrierHP > 0 && damage > 0)
            {
                HandleBarrierDamage(ref damage);
            }
            else if (damage > 0)
            {
                health -= damage; // ���� �������� �� ��������
            }

            // ���������� ��������� �������� � �������� �� ������
            UpdateHealthState();

            // ��������� ������ �������� ����� ��������� �����
            UpdateHealthBar();
        }
    }

    // ����� ��� ��������� �����, ����������� �� ������
    private void HandleBarrierDamage(ref int damage)
    {
        // ���� �������� �� �������
        barrierHP -= damage;

        // ��������, �� �������� �� ������
        if (barrierHP < 0)
        {
            health += barrierHP; // ��������� �������� �� ������� �� �������
            barrierHP = 0;        // �������� ������
            Destroy(buffs.Find(buff => buff.name == "Barrier")); // ������� ������ �������
        }
    }

    // ����� ��� ���������� ��������� �������� �����
    private void UpdateHealthState()
    {
        if (health <= 0)
        {
            health = 0; // ������� ������ ������ ����
            isAlive = false;
        }
        else if (health > maxHealth)
        {
            health = maxHealth; // �������� �� ����� ��������� ��������
        }
    }

    // ����� ��� ��������� �������
    public void ApplyBarrier(int power)
    {
        barrierHP = power; // ������������� �������� �������
    }

    // ����� ��� ���������� ������ �������� � ���������� �����������
    public void UpdateHealthBar()
    {
        healthBar.value = (float)health / maxHealth; // ��������� �������� ��������
        hpText.text = health.ToString();             // ��������� ��������� �����������
    }

    // ����� ��� ������ ��������� ����� (��������, � ������ ����� ����)
    public void ResetUnit()
    {
        health = maxHealth; // ��������������� ������������ ��������
        isAlive = true;     // ��������� ��������� ����� �� "���"
        UpdateHealthBar();  // ��������� UI

        // ����� ���������� �������� �������, ������� � ������ ��������� �����
    }
}
