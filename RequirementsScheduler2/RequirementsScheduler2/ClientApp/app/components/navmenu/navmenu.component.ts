import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthenticationService } from '../../services/authentication.service';
import { Subscription } from 'rxjs/Subscription';
import { isBrowser } from 'angular2-universal';

@Component({
    selector: 'nav-menu',
    template: require('./navmenu.component.html'),
    styles: [require('./navmenu.component.css')]
})
export class NavMenuComponent implements OnInit, OnDestroy {
    isLogged: boolean;
    isAdmin: boolean;

    subscription: Subscription;

    constructor(private _authenticationService: AuthenticationService) { }

    ngOnInit() {
        this.subscription = this._authenticationService.user
            .subscribe(item => {
                if (item != null) {
                    this.isLogged = true;
                    this.isAdmin = item.isAdmin;
                } else {
                    this.isLogged = false;
                }
            });

        if (isBrowser) {
            if (localStorage.getItem('currentUser')) {
                let user = JSON.parse(localStorage.getItem('currentUser'));
                this.isLogged = true;
                this.isAdmin = user.isAdmin;
            }
        }
    }

    ngOnDestroy() {
        // prevent memory leak when component is destroyed
        this.subscription.unsubscribe();
    }
}
