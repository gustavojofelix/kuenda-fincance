import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const activeBranchId = localStorage.getItem('activeBranchId');

  let headers = req.headers;
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }
  if (activeBranchId) {
    headers = headers.set('X-Branch-Id', activeBranchId);
  }

  const authReq = req.clone({ headers });
  return next(authReq);
};
