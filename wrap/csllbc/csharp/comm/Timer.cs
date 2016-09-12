/**
 * @file    Timer.cs
 * @author  Longwei Lai<lailongwei@126.com>
 * @date    2016/06/29
 * @version 1.0
 *
 * @brief   The timer class encapsulation.
 */

using System;

namespace llbc
{
    #region Timer Timeout/Cancel handlers
    public delegate bool TimeoutHandler(Timer timer);
    public delegate void TimerCancelHandler(Timer timer);
    #endregion

    /// <summary>
    /// Timer class.
    /// </summary>
    public class Timer
    {
        #region Ctor/Dtor
        /// <summary>
        /// Construct new timer.
        /// </summary>
        /// <param name="timeoutHandler">timeout handler</param>
        /// <param name="cancelHandler">timer cancel handler, optional</param>
        public Timer(TimeoutHandler timeoutHandler,
                     TimerCancelHandler cancelHandler = null)
        {
            _timeoutHandler = timeoutHandler;
            _cancelHandler = cancelHandler;

            _nativeTimeoutDeleg = _OnTimeout;
            _nativeCancelDeleg = _OnCancel;
            _nativeTimer = LLBCNative.csllbc_Timer_Create(_nativeTimeoutDeleg, _nativeCancelDeleg);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Timer()
        {
            LLBCNative.csllbc_Timer_Destroy(_nativeTimer);
        }
        #endregion // Ctor/Dtor

        #region scheduling, dueTime/period
        /// <summary>
        /// Check this timer scheduling or not.
        /// </summary>
        public bool scheduling
        {
            get
            {
                return LLBCNative.csllbc_Timer_IsScheduling(_nativeTimer) != 0;
            }
        }

        /// <summary>
        /// Get timer dueTime(second).
        /// </summary>
        public double dueTime
        {
            get
            {
                return LLBCNative.csllbc_Timer_GetDueTime(_nativeTimer) / 1000.0;
            }
        }

        /// <summary>
        /// Get timer period(second).
        /// </summary>
        public double period
        {
            get
            {
                return LLBCNative.csllbc_Timer_GetPeriod(_nativeTimer) / 1000.0;
            }
        }
        #endregion // scheduling, dueTime/period

        #region timeoutHandler, cancelHandler
        /// <summary>
        /// Timer timeout handler.
        /// </summary>
        public TimeoutHandler timeoutHandler
        {
            get { return _timeoutHandler; }
        }

        /// <summary>
        /// Timer cancel handler.
        /// </summary>
        public TimerCancelHandler cancelHandler
        {
            get { return _cancelHandler; }
        }
        #endregion

        #region Schedule, Cancel
        /// <summary>
        /// Schedule timer.
        /// </summary>
        /// <param name="dueTime">dueTime, in seconds</param>
        /// <param name="period">period, in seconds, default is 0.0, means same with dueTime</param>
        public void Schedule(double dueTime, double period = 0.0)
        {
            int ret = LLBCNative.csllbc_Timer_Schedule(
                _nativeTimer, (ulong)(dueTime * 1000), (ulong)(period * 1000));
            if (ret != LLBCNative.LLBC_OK)
                throw ExceptionUtil.CreateExceptionFromCoreLib();
        }

        /// <summary>
        /// Schedule timer.
        /// </summary>
        /// <param name="firstTimeoutTime">first timeout time, absolute time</param>
        /// <param name="period">period, in seconds, default is 0.0, means same with (firstTimeoutTime - DateTime.Now)</param>
        public void Schedule(DateTime firstTimeoutTime, double period = 0.0)
        {
            Schedule((firstTimeoutTime - DateTime.Now).TotalSeconds, period);
        }

        /// <summary>
        /// Schedule timer, same with Non-Static Schedule(double, double) method.
        /// </summary>
        /// <param name="timeoutHandler">Timeout handler</param>
        /// <param name="dueTime">dueTime, in seconds</param>
        /// <param name="period">period, in seconds</param>
        /// <param name="cancelHandler">timer cancel handler</param>
        /// <returns>new timer object</returns>
        public static Timer Schedule(TimeoutHandler timeoutHandler,
                                     double dueTime,
                                     double period = 0.0,
                                     TimerCancelHandler cancelHandler = null)
        {
            Timer timer = new Timer(timeoutHandler, cancelHandler);
            timer.Schedule(dueTime, period);

            return timer;
        }

        /// <summary>
        /// Schedule timer, same with Non-Static Schedule(DateTime, double) method.
        /// </summary>
        /// <param name="timeoutHandler">Timeout handler</param>
        /// <param name="firstTimeoutTime">first timeout time</param>
        /// <param name="period">period, in seconds</param>
        /// <param name="cancelHandler">timer cancel handler</param>
        /// <returns>new timer object</returns>
        public static Timer Schedule(TimeoutHandler timeoutHandler,
                                     DateTime firstTimeoutTime,
                                     double period = 0.0,
                                     TimerCancelHandler cancelHandler = null)
        {
            Timer timer = new Timer(timeoutHandler, cancelHandler);
            timer.Schedule(firstTimeoutTime, period);

            return timer;
        }

        /// <summary>
        /// Cancel timer.
        /// </summary>
        public void Cancel()
        {
            LLBCNative.csllbc_Timer_Cancel(_nativeTimer);
        }
        #endregion

        #region Internal implements
        private int _OnTimeout()
        {
            return _timeoutHandler(this) ? 1 : 0;
        }

        private void _OnCancel()
        {
            _cancelHandler(this);
        }
        #endregion // Internal implements

        TimeoutHandler _timeoutHandler;
        TimerCancelHandler _cancelHandler;

        IntPtr _nativeTimer;
        LLBCNative.Deleg_Timer_OnTimeout _nativeTimeoutDeleg;
        LLBCNative.Deleg_Timer_OnCancel _nativeCancelDeleg;
    }
}