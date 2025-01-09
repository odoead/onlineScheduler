import { HttpErrorResponse, HttpEvent, HttpHandler, HttpRequest } from '@angular/common/http';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, Observable, throwError } from 'rxjs';

export function errorHandlingInterceptor(router: Router) {
  return (req: HttpRequest<unknown>,next: HttpHandler): Observable<HttpEvent<unknown>> => {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error) {

          const isPageLoad:boolean= req.headers.get('Page-Load') === 'true';//if error happens while fetching page data then nav to error page instead
          // if error happens with loaded page then print error 

          if (error.status === 400) {
            if (error.error.errors) {
              return throwError(
                () => new Error(JSON.stringify(error.error.errors))
              );
            } else {
              alert(error.error.message);
              //toastr.error(error.error.message, error.status.toString());
            }
          }

          if (error.status === 401) {
            alert(error.error.message);
            //toastr.error(error.error.message, error.status.toString());
          }

          if (error.status === 404) {
            if (isPageLoad) {
            router.navigateByUrl('/404');
            }
           else {
            console.error(error.error.message);
            alert(error.error.message);
          }
        }

          if (error.status === 500) {
            if (isPageLoad) {
            const navigationExtras: NavigationExtras = {state: { error: error.error.message },};
            router.navigateByUrl('/500', navigationExtras);
          }else{
            alert( error.error.message);
          }
        }

        req.headers.delete('Page-Load');//remove unnecessary header
      }
        return throwError(() => new Error(error.message));
      })
    );
  };
}
