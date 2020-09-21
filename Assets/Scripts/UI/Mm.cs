using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Player;
using UnityEngine;
using Utils;

namespace UI
{
    public class StackL<T> : Stack<T>
    {

        public event Action<T> OnPush;
        public event Action<T> OnPop;

        public new void Push(T item)
        {
            OnPush?.Invoke(item);
            base.Push(item);
        }
        
        public new T Pop()
        {
            var item = base.Pop();
            OnPop?.Invoke(item);
            return item;
        }
    }
    
    /// <summary>
    /// Menu manager is useful for complex menus that require decent navigation back and forth
    /// // TODO: this is actually used to stack UIs together, is it ok ?
    /// </summary>
    public class Mm : Singleton<Mm>
    {
        [SerializeField] private UnitSelection unitSelection;
        [SerializeField] private CameraController cameraController;
        
        private readonly StackL<Menu> _stack = new StackL<Menu>();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnEscapeMenu(bool push = false)
        {
            var isEmpty = IsEmpty();
            // Can't select anything while scrolling a menu
            unitSelection.disable = !isEmpty;
            cameraController.disable = !isEmpty;
            // Hide hud when showing any menu (ignore if no experience is set)
            if (Gm.instance.Experience != null) EnableHud(isEmpty && !push);
        }

        public void Push(Menu menu)
        {
            OnEscapeMenu(true);
            if (_stack.Count > 0) _stack.Peek().Hide(); // TODO: By default hide current but maybe in some cases could want to literally stack UIs ?
            _stack.Push(menu);
            menu.Show();
        }

        public Menu Pop()
        {
            var ret = _stack.Pop();
            ret.Hide();
            if (_stack.Count > 0) _stack.Peek().Show();
            OnEscapeMenu();
            return ret;
        }

        /// <summary>
        /// Pop all elements until reaching the given one which is included
        /// If given an in-existing menu, will pop all the stack
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public List<Menu> PopTo(Menu menu)
        {
            var ret = new List<Menu>();
            while (_stack.Count > 0)
            {
                ret.Add(Pop());
                
                // Break the loop once we've popped up to the given menu
                if (ret.Last().Equals(menu)) break;
            }
            return ret;
        }

        public void PopAll()
        {
            while(_stack.Count > 0) Pop();
        }

        public bool IsEmpty()
        {
            return _stack.Count == 0;
        }

        /// <summary>
        /// Show or hide all hud menus
        /// </summary>
        /// <param name="enable"></param>
        public void EnableHud(bool enable)
        {
            if (enable) PopAll();
            foreach (var menu in FindObjectsOfType<Menu>())
            {
                if (!menu.isHud) continue;
                if (enable) menu.Show();
                else menu.Hide();
            }
        }
    }
}
