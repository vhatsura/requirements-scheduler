import { Injectable, OnInit } from '@angular/core';
import { Http, Headers, Response, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { isBrowser } from 'angular2-universal';

import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/of';

import { Token } from '../models/index';
import { AlertService } from './alert.service';

import { tokenNotExpired, JwtHelper } from 'angular2-jwt';

@Injectable()
export class AuthenticationService implements OnInit {

    private jwtHelper = new JwtHelper();

    // Observable navItem source
    private _user : BehaviorSubject<Token> = new BehaviorSubject<Token>(null);

    user = this._user.asObservable();

    constructor(
        private http: Http,
        private alertService: AlertService
    ) { }

    loggedIn(): boolean {
        if (isBrowser) {
            return tokenNotExpired();
        } else {
            return false;
        }
        
    }

    userRole(): string {
        if (isBrowser) {
            var token = localStorage.getItem('id_token');
            let decodedToken = this.jwtHelper.decodeToken(token);
            return decodedToken.role;
        }
        return "";
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
                    if (isBrowser) {
                        // store user details and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('id_token', JSON.stringify(user));
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
        if (isBrowser) {
            // remove user from local storage to log user out
            localStorage.removeItem('currentUser');
            this._user.next(null);
        }
    }

    ngOnInit(): void {
        if (isBrowser) {
            if (localStorage.getItem('currentUser')) {
                let user = JSON.parse(localStorage.getItem('currentUser'));
                this._user.next(user);
            }
        }
    }
}