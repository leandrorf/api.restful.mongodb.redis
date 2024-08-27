import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { map, catchError, retryWhen, delay, take } from 'rxjs/operators';
import { HttpUtilService } from 'src/app/shared/http-util.service';

@Injectable({
  providedIn: 'root'
})
export class HomeService {

  private API_URL = environment.URL;

  constructor(private http: HttpClient, private httpUtil: HttpUtilService) { }
  
  getNews() {
    return this.http.get(this.API_URL + 'newsexternal/1/4')
      .pipe(map(this.httpUtil.extrairDados))
      .pipe(
        retryWhen(errors => errors.pipe(delay(1000), take(10))),
        catchError(this.httpUtil.processarErros));
  }
}
