import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthenticationService } from '../../services/index';
import { Subscription } from 'rxjs/Subscription';
import { isBrowser } from 'angular2-universal';

import { JwtHelper } from 'angular2-jwt';

@Component({
    selector: 'nav-menu',
    template: require('./navmenu.component.html'),
    styles: [require('./navmenu.component.css')]
})
export class NavMenuComponent implements OnInit, OnDestroy {
    isLogged: boolean;
    isAdmin: boolean;

    subscription: Subscription;
    jwtHelper: JwtHelper = new JwtHelper();

    constructor(public authService: AuthenticationService) { }

    ngOnInit() {
        this.subscription = this.authService.user
            .subscribe(item => {
                if (this.authService.loggedIn()) {
                    this.isLogged = true;
                    this.isAdmin = this.authService.userRole() === "admin";
                } else {
                    this.isLogged = false;
                }
            });

        if (this.authService.loggedIn()) {
            this.isLogged = true;
            this.isAdmin = this.authService.userRole() === "admin";
        } else {
            this.isLogged = false;
        }
    }

    ngOnDestroy() {
        // prevent memory leak when component is destroyed
        this.subscription.unsubscribe();
    }
}
