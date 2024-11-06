using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Baff : MonoBehaviour
{
    // ������ �� ������
    public Server servak;

    // UI ������� ��� ����������� �����������������
    public TextMeshProUGUI textDuration;

    // ������, ������� ����� ��������� ��� �������� ������� �����
    public GameObject fxEffect;

    // ����� ���������� ��� ����������� �������
    private void OnDestroy()
    {
        Debug.Log("Destroy: " + name);

        // ������� ������������ ������ Unit, � �������� ����������� ������ ����
        Unit unit = transform.GetComponentInParent<Unit>();
        if (unit == null)
        {
            Debug.LogWarning("Unit component not found in parent chain.");
            return;
        }

        // ������� ���� �� ������ ������
        RemoveBuff(unit.buffs);
        // ������� ������ �� ������ ��������
        RemoveBuff(unit.debuffs);

        // ���������� ���������� ������, ���� �� ����������
        if (fxEffect != null)
        {
            Destroy(fxEffect);
        }
    }

    // ����� ��� �������� ������� ������� �� ������ �������� �������� (������ ��� ��������)
    private void RemoveBuff(List<GameObject> buffList)
    {
        GameObject buffToRemove = buffList.Find(buff => buff.gameObject == gameObject);
        if (buffToRemove != null)
        {
            buffList.Remove(buffToRemove);
        }
    }
}
