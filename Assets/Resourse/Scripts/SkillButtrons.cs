using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Класс для управления отображением кнопок навыков игрока
public class SkillButtons : MonoBehaviour
{
    [SerializeField] private List<ButtonSkills> buttons; // Список кнопок навыков
    [SerializeField] private List<TextMeshProUGUI> textCooldowns; // Список текстов для отображения времени перезарядки
    [SerializeField] private Server server; // Ссылка на сервер для взаимодействия с игроком и противником

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Здесь можно инициализировать начальные состояния, если это необходимо
    }

    // Метод, вызываемый каждый кадр
    void Update()
    {
        UpdateSkillButtons(); // Обновление состояния кнопок навыков
    }

    // Обновление состояния кнопок навыков на основе временных значений перезарядки
    private void UpdateSkillButtons()
    {
        for (int i = 0; i < server.playerUnit.abilities.Count; i++)
        {
            float cooldownTime = server.playerUnit.abilities[i].CooldownTime; // Получаем текущее время перезарядки
            float maxCooldown = server.playerUnit.abilities[i].cooldown; // Получаем максимальное время перезарядки

            // Если перезарядка все еще активна
            if (cooldownTime > 0)
            {
                // Обновляем заполнение кнопки и текст перезарядки
                buttons[i].ButtonImage.fillAmount = 1f - (cooldownTime / maxCooldown);
                textCooldowns[i].text = cooldownTime.ToString("0"); // Форматируем текст с нулями
            }
            else // Если перезарядка завершена
            {
                buttons[i].ButtonImage.fillAmount = 1f; // Заполняем кнопку полностью
                textCooldowns[i].text = "0"; // Сбрасываем текст времени
            }
        }
    }

    // Метод для активации навыка игрока
    public void ActivatePlayerSkill(int skillIndex)
    {
        // Выбор навыка и применение его к противнику
        server.SelectAbility(skillIndex, server.playerUnit, server.enemyUnit);
    }
}

// Класс для описания кнопки навыка
[System.Serializable]
public class ButtonSkills
{
    public string skillName; // Название навыка
    public Image ButtonImage; // Изображение кнопки
    public int indexSkill; // Индекс навыка для идентификации
}
