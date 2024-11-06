using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Baff : MonoBehaviour
{
    // Ссылка на сервер
    public Server servak;

    // UI элемент для отображения продолжительности
    public TextMeshProUGUI textDuration;

    // Эффект, который будет уничтожен при удалении данного баффа
    public GameObject fxEffect;

    // Метод вызывается при уничтожении объекта
    private void OnDestroy()
    {
        Debug.Log("Destroy: " + name);

        // Находим родительский объект Unit, к которому принадлежит данный бафф
        Unit unit = transform.GetComponentInParent<Unit>();
        if (unit == null)
        {
            Debug.LogWarning("Unit component not found in parent chain.");
            return;
        }

        // Удаляем бафф из списка баффов
        RemoveBuff(unit.buffs);
        // Удаляем дебафф из списка дебаффов
        RemoveBuff(unit.debuffs);

        // Уничтожаем визуальный эффект, если он существует
        if (fxEffect != null)
        {
            Destroy(fxEffect);
        }
    }

    // Метод для удаления данного объекта из списка заданных эффектов (баффов или дебаффов)
    private void RemoveBuff(List<GameObject> buffList)
    {
        GameObject buffToRemove = buffList.Find(buff => buff.gameObject == gameObject);
        if (buffToRemove != null)
        {
            buffList.Remove(buffToRemove);
        }
    }
}
