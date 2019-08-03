import { Injectable, Inject, OnInit } from '@angular/core';
import { Http, Headers, Response, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/of';

import { AlertService } from './alert.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

@Injectable()
export class AuthenticationService {

    private jwtHelper = new JwtHelperService();

    // Observable navItem source
    private _user: BehaviorSubject<string> = new BehaviorSubject<string>(null);

    user = this._user.asObservable();

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private http: Http,
        private alertService: AlertService
    ) { }

    loggedIn(): boolean {
        if (isPlatformBrowser(this.platformId)) {
            let token = localStorage.getItem('token');
            var isTokenExpired = this.jwtHelper.isTokenExpired(token);
            return !isTokenExpired;
        } else {
            console.log('Platform isn\'t browser');
            return false;
        }
    }

    userRole(): string {
        if (isPlatformBrowser(this.platformId)) {
            let token = localStorage.getItem('token');
            let decodedToken = this.jwtHelper.decodeToken(token);
            let role = decodedToken.role;
            if (role)
                return role.toLowerCase();
            else
                return '';
        }
        return '';
    }

    login(username: string, password: string) {
        let headers = new Headers({ 'Content-Type': 'application/x-www-form-urlencoded' });
        let options = new RequestOptions({ headers: headers });
        let body = `username=${username}&password=${password}`;
        return this.http.post('/api/token', body, options)
            .map((response: Response) => {
                // login successful if there's a jwt token in the response
                let user = response.json();
                if (user && user.access_token) {
                    if (isPlatformBrowser(this.platformId)) {
                        // store user details and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('token', user.access_token);
                        this._user.next(user);

                        return true;
                    }
                }
                return false;
            })
            .catch((error: Response | any) => {
                // In a real world app, we might use a remote logging infrastructure
                let errMsg: string;
                if (error instanceof Response) {
                    const body = error.json() || '';
                    const err = body.error || JSON.stringify(body);
                    errMsg = `${error.status} - ${error.statusText || ''} ${err}`;
                } else {
                    errMsg = error.message ? error.message : error.toString();
                }
                this.alertService.error(errMsg, true);
                return Observable.of(false);
            });
    }

    logout() {
        if (isPlatformBrowser(this.platformId)) {
            // remove user from local storage to log user out
            localStorage.removeItem('token');
            this._user.next(null);
        }
    }
}
