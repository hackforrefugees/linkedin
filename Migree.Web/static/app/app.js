var migree = angular.module('migreeApp', [
    'ngRoute',
    'ui.router',
    'LocalStorageModule',
    'jcs-autoValidate',
    'ngImgCrop',
    'frapontillo.bootstrap-switch'
]);

migree.config(function ($routeProvider, $locationProvider, $stateProvider, $urlRouterProvider) {

  //routing DOESN'T work without html5Mode
  $locationProvider.html5Mode({
    enabled: true,
    requireBase: false
  });


  $stateProvider
    .state('home', {
      url: '/',
      templateUrl: '/views/start.html',
      controller: 'StartCtrl',
      data: {
        requireLogin: false
      }
    })
    .state('register', {
      url: '/register',
      templateUrl: '/views/register.html',
      controller: 'RegisterCtrl',
      data: {
        requireLogin: false
      }
    })
    .state('thankyou', {
      url: '/thankyou',
      templateUrl: '/views/thankyou.html',
      controller: 'ThankYouCtrl',
      data: {
        requireLogin: false
      }
    })
    .state('login', {
      url: '/login',
      templateUrl: '/views/login.html',
      controller: 'LoginCtrl',
      data: {
        requireLogin: false
      }
    })
    .state('logout', {
      url: '/logout',
      templateUrl: '/views/404.html',
      controller: function () {

      },
      data: {
        requireLogin: true
      }
    })
    .state('dashboard', {
      url: '/dashboard',
      templateUrl: '/views/dashboard.html',
      controller: 'DashboardCtrl',
      data: {
        requireLogin: true
      }
    })
    .state('forgot', {
      url: '/forgot',
      templateUrl: '/views/forgot.html',
      controller: 'ForgotCtrl',
      data: {
        requireLogin: false
      }
    })
    .state('notfound', {
      url: '/notfound',
      templateUrl: '/views/404.html',
      controller: function ($scope) {
        // do something here?
      },
      data: {
        requireLogin: false
      }
    })
    .state('inbox', {
      url: '/inbox',
      templateUrl: '/views/inbox.html',
      controller: 'InboxCtrl',
      data: {
        requireLogin: true
      }
    })
    .state('matches', {
      url: '/matches',
      templateUrl: '/views/404.html',
      controller: function ($scope) {
      },
      data: {
        requireLogin: true
      }
    })
    .state('profile', {
      url: '/profile',
      templateUrl: '/views/profile.html',
      controller: 'ProfileCtrl',
      data: {
        requireLogin: true
      }
    })
    .state('messages', {
      url: '/messages/:id',
      templateUrl: '/views/messages.html',
      controller: 'MessagesCtrl',
      data: {
        requireLogin: true
      }
    })
    .state('about', {
      url: '/about',
      templateUrl: '/views/about.html',
      controller: function ($scope) {
      },
      data: {
        requireLogin: false
      }
    });

  $urlRouterProvider.otherwise('/404');
});

migree.constant('ngAuthSettings', {
  clientId: 'ngAuthApp'
});

migree.config(function ($httpProvider) {
  $httpProvider.interceptors.push('authInterceptorService');
});

migree.run(['AuthenticationService', '$rootScope', 'bootstrap3ElementModifier', function (authService, $rootScope, bootstrap3ElementModifier) {
  $rootScope.apiServiceBaseUri = 'http://migree.azurewebsites.net/';
  $rootScope.apiServiceVersion = 1;
  authService.fillAuthData();
  bootstrap3ElementModifier.enableValidationStateIcons(true);
  $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
    var requireLogin = toState.data.requireLogin;

    if (requireLogin && !authService.authentication.isAuth) {
      event.preventDefault();
      //Show login modal here
    }
  });
}]);

/*===functions===*/

function validateEmail(email) {
  var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
  return re.test(email);
}
