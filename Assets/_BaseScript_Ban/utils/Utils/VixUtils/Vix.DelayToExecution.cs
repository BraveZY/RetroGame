
//***********************************************************************************************************************
//
//文件名(File Name):     DelayToExecution .cs
//
//功能描述(Description): 延迟执行类工具.
//
//作者(Author):		xw
//
//日期(Create Date):	.
//
//修改记录(Revision History):
//			R1：
//				修改作者:
//				修改日期:
//				修改理由:				
//
//***********************************************************************************************************************

using UnityEngine;
using System.Collections;


namespace VixUtils
{

    /// <summary>
    /// 延迟执行 .
    /// </summary>
    public class DelayToExecution : MonoBehaviour
    {

        /// <summary>
        /// 延迟执行.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <returns></returns>
        public static IEnumerator DelayToDo(System.Action _action, float _delaySeconds)
        {
            yield return new WaitForSeconds(_delaySeconds);
            _action();
        }

        /// <summary>
        /// 延迟循环执行.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToLoop(System.Action _action, float _delaySeconds, float loopingTimes)
        {
            int times = 0;
            while (times < loopingTimes)
            {
                yield return new WaitForSeconds(_delaySeconds);
                _action();
                times++;
            }
        }


        /// <summary>
        /// 延迟(真)循环执行.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToLoop(System.Action _action, float _delaySeconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(_delaySeconds);
                _action();
            }
        }


        /// <summary>
        /// 循环(真)执行.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <returns></returns>
        public static IEnumerator LoopToDo(System.Action _action, float _delaySeconds)
        {
            while (true)
            {
                _action();
                yield return new WaitForSeconds(_delaySeconds);
            }
        }

        /// <summary>
        /// 延迟(真,带条件)循环执行.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToLoop(bool _a, System.Action _action, float _delaySeconds)
        {
            while (_a)
            {
                yield return new WaitForSeconds(_delaySeconds);
                _action();
            }
        }


        /// <summary>
        /// 延迟循环执行act1,且最后执行act2.
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_delaySeconds"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToLoopEndDo(System.Action _action, System.Action _actEnd, float _delaySeconds, float loopingTimes)
        {
            int times = 0;
            while (times < loopingTimes)
            {
                yield return new WaitForSeconds(_delaySeconds);
                _action();
                times++;
            }
            _actEnd();
        }


        /// <summary>
        /// 轮流交替执行.
        /// </summary>
        /// <param name="_action1"></param>
        /// <param name="_action2"></param>
        /// <param name="_delaySeconds"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToAlternation(System.Action _action1, System.Action _action2, float _delaySeconds, float loopingTimes)
        {
            int times = 0;
            while (times < loopingTimes)
            {
                _action1();
                yield return new WaitForSeconds(_delaySeconds);
                _action2();
                yield return new WaitForSeconds(_delaySeconds);
                times++;
            }
        }

        /// <summary>
        /// 轮流交替执行.
        /// </summary>
        /// <param name="_action1"></param>
        /// <param name="_action2"></param>
        /// <param name="_delaySeconds1"></param>
        /// <param name="_delaySeconds2"></param>
        /// <param name="loopingTimes"></param>
        /// <returns></returns>
        public static IEnumerator DelayToAlternation(System.Action _action1, System.Action _action2, float _delaySeconds1, float _delaySeconds2, float loopingTimes)
        {
            int times = 0;
            while (times < loopingTimes)
            {
                _action1();
                yield return new WaitForSeconds(_delaySeconds1);
                _action2();
                yield return new WaitForSeconds(_delaySeconds2);
                times++;
            }
        }


    }
}