import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import { isBrowser } from 'angular2-universal';

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(private router: Router) { }

    canActivate() {
        if (isBrowser) {
            if (localStorage.getItem('currentUser')) {
                return true;
            }
        }

        this.router.navigate(['/login']);

        return false;
    }
}