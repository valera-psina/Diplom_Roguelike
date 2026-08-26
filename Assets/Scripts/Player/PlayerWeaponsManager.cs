using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponsManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<WeaponController> weaponInstances = new List<WeaponController>();

    [Header("Parent")]
    [SerializeField] private Transform weaponParent;

    [Header("Input")]
    [SerializeField] private InputActionReference scrollWeaponAction;

    [Header("Animation")]
    [SerializeField] private GameObject AnimateObject;
    [SerializeField] private Transform originalPosition;
    [SerializeField] private Transform downPosition;

    private int currentWeaponIndex = -1;
    private WeaponController currentWeapon;
    private List<WeaponController> weaponPrefabs = new List<WeaponController>();

    private bool isSwitching;
    private bool isReloadingHide;
    private bool isMeleeBlock;
    private Coroutine hideCoroutine;

    public bool PreventShooting => isSwitching || isReloadingHide || isMeleeBlock;
    public event Action<WeaponController> OnWeaponSwitched;
    public WeaponController ActiveWeapon => currentWeapon;
    public int ActiveWeaponIndex => currentWeaponIndex;

    private void Start()
    {
        if (weaponParent == null)
            weaponParent = transform;

        for (int i = 0; i < weaponInstances.Count; i++)
        {
            if (weaponInstances[i] != null)
            {
                SwitchToWeapon(i);
                break;
            }
        }

        if (AnimateObject != null && originalPosition != null)
            AnimateObject.transform.localPosition = originalPosition.localPosition;
    }

    private void OnEnable()
    {
        if (scrollWeaponAction != null)
            scrollWeaponAction.action.performed += OnScroll;
    }

    private void OnDisable()
    {
        if (scrollWeaponAction != null)
            scrollWeaponAction.action.performed -= OnScroll;
    }

    public void SwitchToWeapon(int index)
    {
        if (index < 0 || index >= weaponInstances.Count) return;
        if (weaponInstances[index] == null) return;
        if (currentWeaponIndex == index) return;
        if (isSwitching) return;

        StartCoroutine(SwitchRoutine(index));
    }

    public void SwitchToWeaponByIndex(int index)
    {
        SwitchToWeapon(index);
    }

    public void AddWeapon(WeaponController prefab)
    {
        if (prefab == null) return;

        weaponPrefabs.Add(prefab);

        WeaponController instance = Instantiate(prefab, weaponParent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.gameObject.SetActive(false);
        weaponInstances.Add(instance);

        if (currentWeaponIndex == -1)
            SwitchToWeapon(weaponInstances.Count - 1);
    }

    public List<WeaponController> GetAllWeaponPrefabs()
    {
        return new List<WeaponController>(weaponPrefabs);
    }

    private IEnumerator SwitchRoutine(int index)
    {
        isSwitching = true;

        yield return StartCoroutine(MoveAnimateTo(downPosition.localPosition, 0.15f));

        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        currentWeaponIndex = index;
        currentWeapon = weaponInstances[index];
        currentWeapon.gameObject.SetActive(true);
        OnWeaponSwitched?.Invoke(currentWeapon);

        yield return StartCoroutine(MoveAnimateTo(originalPosition.localPosition, 0.15f));

        isSwitching = false;
    }

    public void HideWeaponForDuration(float duration)
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideRoutine(duration));
    }

    private IEnumerator HideRoutine(float duration)
    {
        isReloadingHide = true;
        yield return StartCoroutine(MoveAnimateTo(downPosition.localPosition, 0.15f));
        yield return new WaitForSeconds(duration - 0.3f);
        yield return StartCoroutine(MoveAnimateTo(originalPosition.localPosition, 0.15f));
        isReloadingHide = false;
    }

    public void StartMeleeBlock()
    {
        isMeleeBlock = true;
        if (AnimateObject != null && downPosition != null)
            AnimateObject.transform.localPosition = downPosition.localPosition;
    }

    public void EndMeleeBlock()
    {
        isMeleeBlock = false;
        if (AnimateObject != null && originalPosition != null)
            AnimateObject.transform.localPosition = originalPosition.localPosition;
    }

    private IEnumerator MoveAnimateTo(Vector3 targetLocalPos, float duration)
    {
        if (AnimateObject == null) yield break;
        float elapsed = 0f;
        Vector3 start = AnimateObject.transform.localPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            AnimateObject.transform.localPosition = Vector3.Lerp(start, targetLocalPos, t);
            yield return null;
        }
        AnimateObject.transform.localPosition = targetLocalPos;
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        float scroll = ctx.ReadValue<float>();
        if (scroll > 0f)
            CycleWeapon(1);
        else if (scroll < 0f)
            CycleWeapon(-1);
    }

    private void CycleWeapon(int direction)
    {
        if (weaponInstances.Count == 0) return;

        int newIndex = currentWeaponIndex;
        int safety = 0;
        do
        {
            newIndex = (newIndex + direction + weaponInstances.Count) % weaponInstances.Count;
            safety++;
        }
        while (weaponInstances[newIndex] == null && safety < weaponInstances.Count);

        SwitchToWeapon(newIndex);
    }
}