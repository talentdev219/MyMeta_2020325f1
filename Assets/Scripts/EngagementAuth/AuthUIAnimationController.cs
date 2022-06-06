using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthUIAnimationController : MonoBehaviour
{
    public GameObject Main, SignIn, Items, Apply, Invites;
    public AnimationClip main_popup_up, main_popup_down, signin_popup_up, signin_popup_down, item_popup_up, item_popup_down;

    public void OpenPage(string action)
    {
        if (action == "main_to_signin") StartCoroutine(MainToSignInAnimation());
        if (action == "signin_to_main") StartCoroutine(SignInToMainAnimation());
        if (action == "main_to_items") StartCoroutine(MainToItemsAnimation());
        if (action == "items_to_main") StartCoroutine(ItemsToMainAnimation());
        if (action == "main_to_apply") StartCoroutine(MainToApplyAnimation());
        if (action == "apply_to_main") StartCoroutine(ApplyToMainAnimation());
        if (action == "main_to_invite") StartCoroutine(MainToInviteAnimation());
        if (action == "invite_to_main") StartCoroutine(InviteToMainAnimation());
    }

    IEnumerator MainToSignInAnimation()
    {
        Main.GetComponent<Animator>().Play("main_popup_down");
        SignIn.SetActive(true);
        SignIn.GetComponent<Animator>().Play("signin_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Main.SetActive(false);
    }

    IEnumerator SignInToMainAnimation()
    {
        SignIn.GetComponent<Animator>().Play("signin_popup_down");
        Main.SetActive(true);
        Main.GetComponent<Animator>().Play("main_popup_up"); //?
        yield return new WaitForSeconds(signin_popup_down.length);
        SignIn.SetActive(false);
    }

    IEnumerator MainToItemsAnimation()
    {
        Main.GetComponent<Animator>().Play("main_popup_down");
        Items.SetActive(true);
        Items.GetComponent<Animator>().Play("item_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Main.SetActive(false);
    }

    IEnumerator ItemsToMainAnimation()
    {
        Items.GetComponent<Animator>().Play("item_popup_down");
        Main.SetActive(true);
        Main.GetComponent<Animator>().Play("main_popup_up"); //?
        yield return new WaitForSeconds(item_popup_down.length);
        Items.SetActive(false);
    }

    IEnumerator MainToApplyAnimation()
    {
        Main.GetComponent<Animator>().Play("main_popup_down");
        Apply.SetActive(true);
        Apply.GetComponent<Animator>().Play("item_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Main.SetActive(false);
    }

    IEnumerator ApplyToMainAnimation()
    {
        Apply.GetComponent<Animator>().Play("item_popup_down");
        Main.SetActive(true);
        Main.GetComponent<Animator>().Play("main_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Apply.SetActive(false);
    }

    IEnumerator MainToInviteAnimation()
    {
        Main.GetComponent<Animator>().Play("main_popup_down");
        Invites.SetActive(true);
        Invites.GetComponent<Animator>().Play("item_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Main.SetActive(false);
    }

    IEnumerator InviteToMainAnimation()
    {
        Invites.GetComponent<Animator>().Play("item_popup_down");
        Main.SetActive(true);
        Main.GetComponent<Animator>().Play("main_popup_up"); //?
        yield return new WaitForSeconds(main_popup_down.length);
        Invites.SetActive(false);
    }
}
