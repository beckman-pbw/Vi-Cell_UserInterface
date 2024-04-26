//using System;
//using System.Diagnostics;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.ServiceModel.Activation.Configuration;
//using System.Threading;
//using System.Threading.Tasks;
//using HawkeyeCoreAPI;
//using Moq;
//using Ninject.Extensions.Logging;
//using NUnit.Framework;
//using ScoutModels;
//using ScoutModels.Admin;
//using ScoutModels.Security;
//using ScoutServices;
//using ScoutUtilities.Enums;
//using TestSupport;

//namespace ScoutServicesTests
//{
//    /// <summary>
//    /// This test cannot be run as part of an automated build. It needs the Postgres database for the backend.
//    /// It needs to have the factory_admin user's password constant in the test to match what is in the database.
//    /// The post build events need to be updated with the below lines, removing the first and adding the second.
//    /// -		xcopy /Y /I "$(SolutionDir)\target\dependencies\Hawkeye" $(TargetDir)</PostBuildEvent>
//    /// +		xcopy /Y /I "$(SolutionDir)\..\ApplicationSource\target\Release" $(TargetDir)</PostBuildEvent>
//    /// Finally, even though the Shutdown() method is called, there appear to be threads still running which causes
//    /// the test runner to prompt to exit them, or prevent proper termination.
//    /// </summary>
//    [TestFixture]
//    public class SecurityManagerTests : BackendTestBase
//    {
//        private readonly AutoResetEvent _threadCoordinator2 = new AutoResetEvent(false);

//        private const string UsernameDevice = "factory_admin";
//        private const string PasswordDevice = "Vi-CELL#01";
//        private const string Username1 = "Username1";
//        private const string Password1 = "Password1";
//        private const string Username2 = "Username2";
//        private const string Password2 = "Password2";
//        private const string Username3 = "Username3";
//        private const string Password3 = "Password3";
//        private Task _thread2Task;
//        private CancellationTokenSource _cancellationSource;

//        [SetUp]
//        public override void Setup()
//        {
//            base.Setup();
//            Debug.WriteLine($"Test Assembly = {Assembly.GetExecutingAssembly().Location}");
//            _cancellationSource = new CancellationTokenSource();

//            // Add the device, user1, and user2 to the database

//            // If test users do not already exist, create them. Cannot created factory_admin or know its password for test.

//            /*
//            if (HawkeyeError.eSuccess != User.GetUserDisplayName(UsernameDevice, out _))
//            {
//                Assert.AreEqual(HawkeyeError.eSuccess, User.AddUserAPI(UsernameDevice, UsernameDevice, PasswordDevice, UserPermissionLevel.eNormal));
//            }
//            */

//            // Login as the device user.
//            var hawkeyeResult = UserModel.LoginUser(UsernameDevice, PasswordDevice);
//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(UsernameDevice, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.Setup(): Main threadId = {GetCurrentWin32ThreadId()}, verified that thread1 is showing the device user.");

//            if (HawkeyeError.eSuccess != User.GetUserDisplayName(Username1, out _))
//            {
//                Assert.AreEqual(HawkeyeError.eSuccess, User.AddUserAPI(Username1, Username1, Password1, UserPermissionLevel.eNormal));
//            }

//            if (HawkeyeError.eSuccess != User.GetUserDisplayName(Username2, out _))
//            {
//                Assert.AreEqual(HawkeyeError.eSuccess, User.AddUserAPI(Username2, Username2, Password2, UserPermissionLevel.eNormal));
//            }

//            if (HawkeyeError.eSuccess != User.GetUserDisplayName(Username3, out _))
//            {
//                Assert.AreEqual(HawkeyeError.eSuccess, User.AddUserAPI(Username3, Username3, Password3, UserPermissionLevel.eNormal));
//            }
//        }

//        [TearDown]
//        public override void Cleanup()
//        {
//            if (null != _thread2Task && ! _thread2Task.IsCompleted)
//            {
//                _threadCoordinator2.Set();
//                _cancellationSource.Cancel();
//            }

//            base.Cleanup();
//        }

//        [DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
//        public static extern Int32 GetCurrentWin32ThreadId();

//        [Test, Ignore("Cannot run without database")]
//        public void TestMultipleUsers()
//        {
//            // This is thread 1
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}");

//            _thread2Task = Task.Run(UserThread2, _cancellationSource.Token);
//            var hawkeyeResult = SecurityManager.PushUser(Username1, Password1);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Pushed Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            var curUserId = LoggedInUser.CurrentUser.UserID;
//            Assert.AreEqual(Username1, curUserId);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that thread1 is showing Username1 which just got pushed.");

//            _threadCoordinator2.Set(); // Unblock Stop#1
//            _mainThreadCoordinator.WaitOne(); // Stop#2

//            Assert.AreEqual(Username1, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that thread1 is showing Username1, after thread2 pushed Username2 on its thread");

//            hawkeyeResult = SecurityManager.PopUser();

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Popped Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(UsernameDevice, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that thread1 is showing device user after popping Username1 off the stack");

//            _threadCoordinator2.Set(); // Unblock Stop#3
//            _mainThreadCoordinator.WaitOne(); // Stop#4

//            // Is the user still the device user after thread 2 pushed a second user on its stack?
//            Assert.AreEqual(UsernameDevice, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified the user is still the device user after thread 2 pushed a second user on its stack");

//            // Try to pop the device user off the stack - should fail and do nothing. The user should
//            // still be the device user.
//            hawkeyeResult = SecurityManager.PopUser();

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Invalid pop");

//            Assert.AreEqual(HawkeyeError.eNotPermittedAtThisTime, hawkeyeResult);
//            Assert.AreEqual(UsernameDevice, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that trying to pop the last user of the stack fails and that it is still the device user");

//            _threadCoordinator2.Set(); // Unblock Stop#5
//            _mainThreadCoordinator.WaitOne(); // Stop#6

//            hawkeyeResult = SecurityManager.PushUser(Username1, Password1);

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Pushed Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(Username1, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that the pushed Username1 is the current user");

//            // Check that the transient user is still user 1 after the device user has logged out.
//            UserModel.LogOutUser();

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Logged out device user");

//            Assert.AreEqual(Username1, LoggedInUser.CurrentUser.UserID);
//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()}, verified that after logging out the device user, the pushed Username1 is the current user");

//            hawkeyeResult = SecurityManager.PopUser();

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Popped Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.IsTrue(string.IsNullOrEmpty(LoggedInUser.CurrentUser.UserID));

//            hawkeyeResult = UserModel.LoginUser(Username3, Password3);

//            Debug.WriteLine($"SecurityManagerTests.TestMultipleUsers(): Thread 1 threadId = {GetCurrentWin32ThreadId()} Logged in Username3");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(Username3, LoggedInUser.CurrentUser.UserID);

//            _threadCoordinator2.Set(); // Unblock Stop#7
//            _mainThreadCoordinator.WaitOne(); // Stop#8
//        }

//        private void Thread2Wait()
//        {
//            _threadCoordinator2.WaitOne();
//            if (_cancellationSource.IsCancellationRequested)
//            {
//                throw new Exception("Test aborted");
//            }
//        }

//        public void UserThread2()
//        {
//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}");

//            Thread2Wait(); // Stop #1
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, UsernameDevice);
//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, verified that even though thread1 pushed Username1 on the stack, the device user is showing on thread2.");

//            var hawkeyeResult = SecurityManager.PushUser(Username2, Password2);

//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, pushed Username2");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username2);
//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, verified that pushed Username2 is showing on thread2.");

//            _mainThreadCoordinator.Set(); // Unblock Stop#2
//            Thread2Wait(); // Stop#3

//            // Verify that this thread's user is unchanged after thread 1 popped its transient user
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username2);

//            // Push another transient user on the stack and check it.
//            hawkeyeResult = SecurityManager.PushUser(Username1, Password1);

//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, pushed Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username1);
//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, verified that pushed Username1 is showing on thread2.");

//            _mainThreadCoordinator.Set(); // Unblock Stop#4
//            Thread2Wait(); // Stop#5

//            // Should still be the same user after the failed attempt to pop the user off of thread1
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username1);
//            hawkeyeResult = SecurityManager.PopUser();

//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, popped Username1");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username2);

//            _mainThreadCoordinator.Set(); // Unblock Stop#6
//            Thread2Wait(); // Stop#7

//            hawkeyeResult = SecurityManager.PopUser();

//            Debug.WriteLine($"SecurityManagerTests.UserThread2(): Thread 2 threadId = {GetCurrentWin32ThreadId()}, popped Username2");

//            Assert.AreEqual(HawkeyeError.eSuccess, hawkeyeResult);
//            Assert.AreEqual(LoggedInUser.CurrentUser.UserID, Username3, "The logged in user has changed and the device principal should show that.");

//            // Allow test to finish
//            _mainThreadCoordinator.Set(); // Unblock Stop#8
//        }
//    }
//}