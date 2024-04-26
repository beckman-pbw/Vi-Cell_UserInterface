using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using ApiProxies.Misc;
using Moq;
using Ninject;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace TestSupport
{
    public class BaseTest
    {
        protected string Username = "username";
        protected string Password = "password";
        protected StandardKernel Kernel = new StandardKernel();

        protected void SetUser(string username, UserPermissionLevel userRole)
        {
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", new UserDomain
            {
                UserID = username,
                RoleID = userRole
            });
        }

        /// <summary>
        /// This method allows you to use a mock ISampleProcessingService. Normally,
        /// any call to Subscribe...() would fail but this sets up a default subscription.
        /// The subscription is fake, however, so do not expect it to do anything.
        /// This is solely to be able to use a mocked ISampleProcessingService.
        /// </summary>
        /// <param name="sampleProcessingServiceMock"></param>
        protected void SetupNullSampleProcessingSubscriptions(Mock<ISampleProcessingService> sampleProcessingServiceMock)
        {
            sampleProcessingServiceMock
                .Setup(m => m.SubscribeToSampleCompleteCallback())
                .Returns(new Observable<ApiEventArgs<SampleEswDomain, SampleRecordDomain>>());
            sampleProcessingServiceMock
                .Setup(m => m.SubscribeToSampleStatusCallback())
                .Returns(new Observable<ApiEventArgs<SampleEswDomain>>());
            sampleProcessingServiceMock
                .Setup(m => m.SubscribeToImageResultCallback())
                .Returns(new Observable<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>>());
            sampleProcessingServiceMock
                .Setup(m => m.SubscribeToWorkListCompleteCallback())
                .Returns(new Observable<ApiEventArgs<List<uuidDLL>>>());
        }
    }

    public class Observable<T> : IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            var s = new Subject<T>();
            return s;
        }
    }
}