using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    [Header("Игрок и Враг")]
    public Unit playerUnit; // Игрок
    public Unit enemyUnit; // Враг

    [Header("UI и Ходы")]
    public GameObject PlayerBlockUI; // UI для блокировки ходов игрока
    public bool PlayerHod; // Флаг, указывающий, чей ход (игрока или врага)

    [Header("Баффы и Дебаффы")]
    public List<BafAndDebaf> _BafAndDebaf; // Список всех баффов и дебаффов
    [SerializeField] private GameObject baffPrefab; // Префаб для создания баффов и дебаффов

    [Header("Управление состоянием игры")]
    private bool cleansingActive; // Активен ли эффект очищения
    [SerializeField] private int currentTurn = 1; // Текущий ход в игре

    private void Awake()
    {
        // Запускаем главный игровой цикл
        StartCoroutine(GameLoop());
    }
    private IEnumerator GameLoop()
    {
        // Главный игровой цикл
        while (playerUnit.isAlive && enemyUnit.isAlive)
        {
            if (currentTurn % 2 == 0) // Ход игрока
            {
                cleansingActive = false; // Отключаем очищение
                yield return new WaitForSeconds(1.5f); // Задержка перед ходом врага
                Debug.Log("Ход врага");

                // Выбор способности врага
                int randomAbilityIndex = Random.Range(0, enemyUnit.abilities.Count);
                while (enemyUnit.abilities[randomAbilityIndex].CooldownTime > 0)
                {
                    randomAbilityIndex = Random.Range(0, enemyUnit.abilities.Count);
                    yield return new WaitForSeconds(0.1f);
                }

                // Используем способность врага
                SelectAbility(randomAbilityIndex, enemyUnit, playerUnit);
                currentTurn++;
                PlayerHod = true; // Возвращаем ход игроку

                playerUnit.UpdateHealthBar(); // Обновляем здоровье игрока
                enemyUnit.UpdateHealthBar(); // Обновляем здоровье врага
            }
            else // Ход врага
            {
                cleansingActive = false; // Отключаем очищение
                Debug.Log("Ход игрока");
                PlayerBlockUI.SetActive(false); // Разблокируем UI

                // Ожидаем, пока игрок совершает действие
                while (PlayerHod)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // Переход к следующему ходу
                currentTurn++;
                PlayerBlockUI.SetActive(true); // Блокируем UI для игрока
                playerUnit.UpdateHealthBar(); // Обновляем здоровье игрока
                enemyUnit.UpdateHealthBar(); // Обновляем здоровье врага
            }
        }

        // Логика окончания игры
        Debug.Log(playerUnit.isAlive ? "Win" : "Game Over");
        yield return new WaitForSeconds(1f);
        ResetGame();
        yield return null;
    }

    public void SelectAbility(int index, Unit caster, Unit target)
    {
        // Проверяем валидность индекса и используем способность
        if (index >= 0 && index < caster.abilities.Count)
        {
            UseAbility(caster.abilities[index], caster, target);
            PlayerHod = false; // Ход игрока завершен
        }
    }

    private IEnumerator Cooldown(Ability ability)
    {
        // Управление временем перезарядки способности
        int cooldownEndTurn = currentTurn + ability.cooldown + 1;

        while (cooldownEndTurn > currentTurn)
        {
            yield return new WaitForSeconds(0.1f);
            ability.CooldownTime = cooldownEndTurn - currentTurn; // Обновляем время до окончания перезарядки
        }

        yield return null;
    }

    private IEnumerator LongPower(Ability ability, Unit target, GameObject buff)
    {
        // Длительное действие способности
        int durationEndTurn = currentTurn + ability.duration + 1;
        int lastTurn = currentTurn;

        // Устанавливаем текст длительности эффекта
        buff.GetComponent<Baff>().textDuration.text = ability.duration.ToString();

        while (durationEndTurn > currentTurn)
        {
            // Проверяем на наличие очищения
            if (cleansingActive && ability.name == "FireBall")
            {
                Debug.Log("Удаление LongPower Огненного шара");
                cleansingActive = false; // Очищение актуально
                StartCoroutine(EndBuffAndDebuff(ability.name, target)); // Завершаем эффект
                yield break; // Завершаем выполнение корутины
            }

            yield return new WaitForSeconds(0.1f);
            if (lastTurn < currentTurn)
            {
                // Наносим урон цели, если это не барьер
                if (ability.name != "Barrier")
                {
                    target.TakeDamage(ability.LongPower);
                    if (ability.FVXPrefab != null)
                        Instantiate(ability.FVXPrefab, target.transform.position, Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }

                lastTurn = currentTurn; // Обновляем последний ход

                if (buff != null)
                {
                    // Обновляем текст длительности эффекта
                    buff.GetComponent<Baff>().textDuration.text = (durationEndTurn - lastTurn).ToString();
                }
                else
                {
                    StartCoroutine(EndBuffAndDebuff(ability.name, target)); // Завершаем эффект
                    yield break; // Завершаем выполнение корутины
                }
            }
        }

        // Завершаем действие баффа
        StartCoroutine(EndBuffAndDebuff(ability.name, target));
        yield return null;
    }

    private IEnumerator EndBuffAndDebuff(string name, Unit target)
    {
        // Завершение действия баффа или дебаффа
        bool isEnded = false;
        int completedChecks = 0;

        while (!isEnded)
        {
            // Удаляем дебаффы
            for (int i = 0; i < target.debuffs.Count; i++)
            {
                if (target.debuffs[i].name == name)
                {
                    Destroy(target.debuffs[i]); // Удаляем дебафф
                    isEnded = true; // Дебафф завершен
                }
                if (i == target.debuffs.Count - 1) completedChecks++; // Проверяем, обработаны ли все дебаффы
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.1f);

            // Удаляем баффы
            for (int i = 0; i < target.buffs.Count; i++)
            {
                if (target.buffs[i].name == name)
                {
                    Destroy(target.buffs[i]); // Удаляем бафф
                    isEnded = true; // Бафф завершен
                }
                yield return new WaitForSeconds(0.1f);
                if (i == target.buffs.Count - 1) completedChecks++; // Проверяем, обработаны ли все баффы
            }

            // Проверяем, завершены ли оба процесса
            isEnded = completedChecks == 2;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    private void UseAbility(Ability ability, Unit caster, Unit target)
    {
        // Использование способности
        if (ability.CooldownTime > 0)
        {
            Debug.Log("Способность в перезарядке!"); // Способность на кд
            return; // Выход, если способность на перезарядке
        }

        switch (ability.name)
        {
            case "Attack":
                // Атака
                Debug.Log("Атака");
                caster.animator.SetTrigger("Attack"); // Запускаем анимацию атаки
                target.TakeDamage(ability.Power); // Наносим урон
                ability.CooldownTime = ability.cooldown; // Устанавливаем время кд
                StartCoroutine(Cooldown(ability)); // Запускаем корутину для отслеживания перезарядки
                break;

            case "Barrier":
                // Создание барьера
                Debug.Log("Барьер");
                Destroy(caster.buffs.Find(buff => buff.gameObject.name == "Barrier")); // Удаляем существующий барьер
                GameObject barrierBuff = Instantiate(baffPrefab, caster.buffPanel); // Создаем новый бафф
                InitializeBuff(ability, caster, barrierBuff); // Инициализируем бафф
                StartCoroutine(LongPower(ability, caster, barrierBuff)); // Запускаем длительное действие


                if (ability.FVXPrefab != null)
                    barrierBuff.GetComponent<Baff>().fxEffect = Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity);


                caster.ApplyBarrier(ability.Power); // Применяем эффект барьера
                ability.CooldownTime = ability.cooldown; // Устанавливаем время кд
                StartCoroutine(Cooldown(ability)); // Запускаем корутину
                break;

            case "Regeneration":
                // Регенирация
                Debug.Log("Регенирация");
                caster.TakeDamage(ability.Power); // Наносим урон
                GameObject regenBuff = Instantiate(baffPrefab, caster.buffPanel); // Создаем новый бафф
                InitializeBuff(ability, caster, regenBuff); // Инициализируем бафф
                StartCoroutine(LongPower(ability, caster, regenBuff)); // Запускаем длительное действие

                if (ability.FVXPrefab != null)
                    Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity); // Создаем эффект визуализации

                ability.CooldownTime = ability.cooldown; // Устанавливаем время кд
                StartCoroutine(Cooldown(ability)); // Запускаем корутину
                break;

            case "FireBall":
                // Огненный шар
                Debug.Log("Огненный шар");
                GameObject fireballDebuff = Instantiate(baffPrefab, target.buffPanel); // Создаем новый дебафф
                InitializeDebuff(ability, target, fireballDebuff); // Инициализируем дебафф

                target.TakeDamage(ability.Power + ability.LongPower); // Наносим урон
                StartCoroutine(LongPower(ability, target, fireballDebuff)); // Запускаем длительное действие

                ability.CooldownTime = ability.cooldown; // Устанавливаем время кд
                StartCoroutine(Cooldown(ability)); // Запускаем корутину
                break;

            case "Cleansing":
                // Очищение
                Debug.Log("Очищение");
                cleansingActive = true; // Активируем очищение
                ability.CooldownTime = ability.cooldown; // Устанавливаем время кд

                if (ability.FVXPrefab != null)
                    Instantiate(ability.FVXPrefab, caster.transform.position, Quaternion.identity); // Создаем эффект визуализации

                StartCoroutine(EndBuffAndDebuff("FireBall", caster)); // Убираем дебаффы
                StartCoroutine(Cooldown(ability)); // Запускаем корутину
                break;
        }
    }

    private void InitializeBuff(Ability ability, Unit caster, GameObject buff)
    {
        // Инициализация баффа
        BafAndDebaf buffData = _BafAndDebaf.Find(b => b.Name == ability.name);
        if (buffData.Sprite != null)
            buff.GetComponent<Image>().sprite = buffData.Sprite; // Устанавливаем спрайт

        buff.GetComponent<Image>().color = buffData.Color; // Устанавливаем цвет
        buff.name = buffData.name; // Устанавливаем имя
        buff.GetComponent<Baff>().servak = this; // Ссылка на сервер
        caster.buffs.Add(buff); // Добавляем бафф к игроку
    }

    private void InitializeDebuff(Ability ability, Unit target, GameObject debuff)
    {
        // Инициализация дебаффа
        BafAndDebaf debuffData = _BafAndDebaf.Find(b => b.Name == ability.name);
        if (debuffData.Sprite != null)
            debuff.GetComponent<Image>().sprite = debuffData.Sprite; // Устанавливаем спрайт

        debuff.GetComponent<Image>().color = debuffData.Color; // Устанавливаем цвет
        debuff.name = debuffData.name; // Устанавливаем имя
        debuff.GetComponent<Baff>().servak = this; // Ссылка на сервер
        target.debuffs.Add(debuff); // Добавляем дебафф к врагу
    }

    public void ResetGame()
    {
        // Сброс игры
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Перезагружаем текущую сцену
    }
}
