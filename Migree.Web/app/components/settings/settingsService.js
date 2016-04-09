﻿migree.factory('settingsService', ['apiService', '$q', function (apiService, $q) {

  var imageUpload = function (file) {
    var deferred = $q.defer();
    if(file) {
      var formData = new FormData();
      formData.append('Content', file);

      apiService.imageUpload(formData).then(function(data) {
        deferred.resolve();
      }, function(fail) {
        deferred.reject();
      });
    }
    else {
      deferred.reject({'status': 'No image provided'});
    }
    return deferred.promise;
  };

  return {
    user: apiService.user,
    competencePromise: apiService.competence.query().$promise,
    locationPromise: apiService.location.query().$promise,
    imageUpload: imageUpload
  };
}]);
