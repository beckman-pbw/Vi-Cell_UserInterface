# Writing Tests that use the Backend {#tests_with_the_backend}

The TestSupport project contains the BackendTestBase class. Tests that require the backend,
which is most service layer tests require, should extend this base class. The Setup() method
of the base class leverages the HardwareManager class, and will block until the backend is
fully initialized. If your test also performs setup, merely override the Setup() method,
call the base method, followed by your tests additional initialization. The Cleanup() method
calls the ShutdownAPI(). Again, if your test requires tear down, override the Cleanup()
method and as your last step, call the base method. Note: At the moment, there is a bug (for which a story has been
created) resulting from threads in the backend that have not completed, and thus leave the
test in a hanging state.

When writing tests that use the backend, the test needs to wait for the backend to be initialized.
This can take 10-15 seconds and 5-6 progress messages maybe generated during this process. It may
be helpful to understand initialization process. The base class contains the following two
fields.
```csharp
protected readonly AutoResetEvent _mainThreadCoordinator = new AutoResetEvent(false);
protected readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
```
The _mainThreadCoordinator can be used if your test needs to block the main test thread,
otherwise, ignore it. You can also reuse the _mockLogger if your test classes need one.

An Assert will be thrown if the initialization process fails.
