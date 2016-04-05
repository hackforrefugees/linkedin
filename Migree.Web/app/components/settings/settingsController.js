﻿migree.controller('settingsController', ['$scope', 'settingsService', '$q', 'fileReader',
  function ($scope, settingsService, $q, fileReader) {
    settingsService.user.query().$promise.then(function (data) {
      $scope.settings = data;
      var promises = [
            settingsService.competencePromise,            
            settingsService.locationPromise
      ];

      $q.all(promises).spread(function (competences, locations) {
        $scope.businesses = [];
        $scope.competences = [];

        $.each(competences, function (key, businessGroup) {
          $scope.businesses.push(businessGroup.business);

          $.each(businessGroup.competences, function (innerKey, competence) {
            $scope.competences.push(competence);
          });
        });

        $scope.businesses.selected = $scope.businesses.filter(function (business) {
          return business.id === $scope.settings.business;
        })[0];

        $scope.locations = locations;
        $scope.locations.selected = $scope.locations.filter(function (location) {
          return location.id === $scope.settings.userLocation;
        })[0];

        $scope.settings.competences.selected = getFilteredArray($scope.competences, $scope.settings.competences);
      });
    });

    $scope.croppedImg = null;
    $scope.srcImg = null;
    $scope.avatarCropped = false;

    var profileFile = null;

    $scope.crop = function () {
      $scope.avatarCropped = true;
    };

    $scope.getFile = function () {
      fileReader.readAsDataUrl($scope.file, $scope).then(function (result) {
        profileFile = $scope.file;
        $scope.srcImg = result;
        $scope.didSelect = true;
      });
    };

    $scope.update = function () {

      var dataURLtoBlob = function(dataurl) {
        var arr = dataurl.split(','),
            mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);

        while(n--){
            u8arr[n] = bstr.charCodeAt(n);
        }

        return new Blob([u8arr], {type:mime});
      };

      var data = $scope.croppedImg;

        settingsService.user.update($scope.settings).$promise.then(function (response) {
          (function(data, scope) {
            var cropped = dataURLtoBlob(data);
            settingsService.imageUpload(cropped).then(
              function(success) {
                window.location.reload();
              }, function(fail) {
                window.alert('oh no!');
              });
          }(data, $scope));
        });

    };

    function getFilteredArray(inputArray, filter) {
      var filteredArray = [];
      for (var i = 0; i < inputArray.length; i++) {
        for (var p = 0; p < filter.length; p++) {
          if (filter[p] === inputArray[i].id) {
            filteredArray.push(inputArray[i]);
          }
        }
      }
      return filteredArray;
    }
  }]);
