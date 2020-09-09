using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly StackL<Menu> _stack = new StackL<Menu>();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            _stack.OnPush += menu => menu.Show();
            _stack.OnPop += menu => menu.Hide();
        }

        public void Push(Menu menu)
        {
            _stack.Push(menu);
        }

        public Menu Pop()
        {
            return _stack.Pop();
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
    }
}
