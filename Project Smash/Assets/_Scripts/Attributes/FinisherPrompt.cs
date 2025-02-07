﻿using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace PSmash.Attributes
{
    public class FinisherPrompt : MonoBehaviour
    {
        [SerializeField] SpriteRenderer controlSprite = null;
        [SerializeField] SpriteRenderer keyboardSprite = null;
        [SerializeField] SpriteRenderer redlightSprite = null;
        PlayerInput playerInput;

        // Start is called before the first frame update
        void Awake()
        {
            if (TryGetComponent(out EnemyPosture posture))
                Destroy(posture.gameObject);
            controlSprite.color = new Color(1, 1, 1, 0);
            keyboardSprite.color = new Color(1, 1, 1, 0);
            redlightSprite.color = new Color(1, 1, 1, 0);
            Transform controller = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0);
            playerInput = controller.GetComponent<PlayerInput>();
            CheckControlInput(playerInput);
        }

        private void Update()
        {
            transform.rotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            playerInput.onControlsChanged += Input_onControlsChanged;
        }

        private void OnDisable()
        {
            playerInput.onControlsChanged -= Input_onControlsChanged;
        }

        private void Input_onControlsChanged(PlayerInput obj)
        {
            //print("Input Scheme Changed");
            CheckControlInput(obj);
        }

        private void CheckControlInput(PlayerInput obj)
        {
            if (obj.currentControlScheme == "Gamepad")
            {
                //print("Change to Gamepad");
                ShowGamepadPrompt();
            }
            else
            {
                //print("Change to Keyboard");
                ShowKeyboardPrompt();
            }
        }

        void ShowGamepadPrompt()
        {
            controlSprite.enabled = true;
            keyboardSprite.enabled = false;
        }

        void ShowKeyboardPrompt()
        {
            controlSprite.enabled = false;
            keyboardSprite.enabled = true;
        }


        public void ShowPrompt()
        {
            StartCoroutine(ShowFinisherPrompt(controlSprite));
            StartCoroutine(ShowFinisherPrompt(keyboardSprite));
            StartCoroutine(ShowFinisherPrompt(redlightSprite));
        }

        private IEnumerator ShowFinisherPrompt(SpriteRenderer spriteRenderer)
        {
            yield return FadeOut(spriteRenderer, 0.5f);
            yield return new WaitForSeconds(1.5f);
            yield return FadeIn(spriteRenderer, 1);
        }

        Coroutine FadeIn(SpriteRenderer spriteRenderer, float time)
        {
            return Fade(spriteRenderer, 0, time);
        }

        Coroutine FadeOut(SpriteRenderer spriteRenderer, float time)
        {
            return Fade(spriteRenderer, 1, time);
        }

        Coroutine Fade(SpriteRenderer spriteRenderer, float target, float time)
        {
            Coroutine coroutine = StartCoroutine(Fading(spriteRenderer, target, time));
            return coroutine;
        }

        IEnumerator Fading(SpriteRenderer spriteRenderer, float target, float time)
        {
            float alpha = spriteRenderer.color.a;
            while (!Mathf.Approximately(alpha, target))
            {
                alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime / time);
                spriteRenderer.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
        }
    }
}

