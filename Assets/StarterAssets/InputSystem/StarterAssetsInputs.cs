using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;

		// Removed:
		public bool jump = false;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		[Header("Additions")]
		public bool aim;
		public bool dodge;
		public bool lightAttack;
		public bool heavyAttack;
		public bool activate;
		public bool reload;
		public bool pause;
		public bool inventory;
		public bool menuLeft;
		public bool menuRight;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		//public void OnSprint(InputValue value)
		//{
		//	SprintInput(value.isPressed);
		//}

		public void OnLightAttack(InputValue value)
		{
			LightAttackInput(value.isPressed);
		}

        public void OnHeavyAttack(InputValue value)
        {
            HeavyAttackInput(value.isPressed);
        }

        public void OnActivate(InputValue value)
		{
			ActivateInput(value.isPressed);
		}

		public void OnDodge(InputValue value)
		{
			DodgeInput(value.isPressed);
		}

        public void OnReload(InputValue value)
        {
            ReloadInput(value.isPressed);
        }

		public void OnPause(InputValue value)
		{
			PauseInput(value.isPressed);
		}

		public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}

		public void OnInventory(InputValue value)
		{
			InventoryInput(value.isPressed);
		}

		public void OnMenuLeft(InputValue value)
		{
			MenuLeftInput(value.isPressed);
		}

        public void OnMenuRight(InputValue value)
        {
            MenuRightInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		//public void SprintInput(bool newSprintState)
		//{
		//	sprint = newSprintState;
		//}

		public void LightAttackInput(bool newLightAttackState)
		{
			lightAttack = newLightAttackState;
		}

        public void HeavyAttackInput(bool newHeavyAttackState)
        {
            heavyAttack = newHeavyAttackState;
        }

        public void ActivateInput(bool newActivateState)
		{
			activate = newActivateState;
		}

		public void DodgeInput(bool newDodgeState)
		{
			dodge = newDodgeState;
		}

        public void ReloadInput(bool newReloadState)
        {
            reload = newReloadState;
        }

        public void PauseInput(bool newPauseState)
        {
            pause = newPauseState;
        }

		public void AimInput(bool newAimState)
		{
			aim = newAimState;
		}

		public void InventoryInput(bool newInventoryState)
		{
			inventory = newInventoryState;
		}

        public void MenuLeftInput(bool newMenuLeftState)
        {
            menuLeft = newMenuLeftState;
        }

        public void MenuRightInput(bool newMenuRightState)
        {
            menuRight = newMenuRightState;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}