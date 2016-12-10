import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { AlertService } from '../../services/alert.service';
import { User } from '../../models/user';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'users',
    template: require('./users.component.html')
})
export class UsersComponent implements OnInit {

    users: User[] = [];

    constructor(private _userService: UserService, private _alertService: AlertService) { }

    ngOnInit() {
        this._userService.getAll()
            .subscribe(
            data => {
                this.users = data;
            },
            error => {
                this._alertService.error(error);
            });
    }
}