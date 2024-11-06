using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [Header("Свойства Здоровья")]
    public int health = 20;                // Текущее здоровье юнита
    public int maxHealth = 20;             // Максимальное здоровье юнита
    public bool isAlive = true;             // Флаг, указывающий, жив юнит или нет

    [Header("Элементы UI")]
    public Slider healthBar;                // Полоска здоровья
    [SerializeField] private TextMeshProUGUI hpText; // Текстовое отображение здоровья

    public Animator animator;                // Аниматор для управления анимациями
    public List<Ability> abilities;         // Список способностей юнита
    public int barrierHP;                   // Здоровье барьера
    public Transform buffPanel;             // Панель для отображения баффов

    [Header("Эффекты")]
    public List<GameObject> debuffs;       // Список дебаффов
    public List<GameObject> buffs;          // Список баффов

    private void Start()
    {
        // Получаем компонент аниматора и обновляем полосу здоровья при старте
        animator = GetComponent<Animator>();
        UpdateHealthBar();
    }

    // Метод для получения урона
    public void TakeDamage(int damage)
    {
        // Проверка, жив ли юнит
        if (isAlive)
        {
            // Обработка урона в зависимости от наличия барьера
            if (barrierHP > 0 && damage > 0)
            {
                HandleBarrierDamage(ref damage);
            }
            else if (damage > 0)
            {
                health -= damage; // Урон проходит по здоровью
            }

            // Обновление состояния здоровья и проверка на смерть
            UpdateHealthState();

            // Обновляем полосу здоровья после получения урона
            UpdateHealthBar();
        }
    }

    // Метод для обработки урона, приходящего на барьер
    private void HandleBarrierDamage(ref int damage)
    {
        // Урон проходит по барьеру
        barrierHP -= damage;

        // Проверка, не сломался ли барьер
        if (barrierHP < 0)
        {
            health += barrierHP; // Уменьшаем здоровье на остаток от барьера
            barrierHP = 0;        // Обнуляем барьер
            Destroy(buffs.Find(buff => buff.name == "Barrier")); // Удаляем объект барьера
        }
    }

    // Метод для обновления состояния здоровья юнита
    private void UpdateHealthState()
    {
        if (health <= 0)
        {
            health = 0; // Умирать нельзя меньше нуля
            isAlive = false;
        }
        else if (health > maxHealth)
        {
            health = maxHealth; // Здоровье не может превышать максимум
        }
    }

    // Метод для установки барьера
    public void ApplyBarrier(int power)
    {
        barrierHP = power; // Устанавливаем здоровье барьера
    }

    // Метод для обновления полосы здоровья и текстового отображения
    public void UpdateHealthBar()
    {
        healthBar.value = (float)health / maxHealth; // Обновляем значение слайдера
        hpText.text = health.ToString();             // Обновляем текстовое отображение
    }

    // Метод для сброса состояния юнита (например, в начале новой игры)
    public void ResetUnit()
    {
        health = maxHealth; // Восстанавливаем максимальное здоровье
        isAlive = true;     // Обновляем состояние юнита на "жив"
        UpdateHealthBar();  // Обновляем UI

        // Также необходимо сбросить статусы, эффекты и другие параметры юнита
    }
}
