﻿migree.controller('notFoundController', ['$scope', 'languageService',
  function ($scope, languageService) {

    languageService.then(function (data) {
      $scope.language = data.notFound;
    });    
  }]);
