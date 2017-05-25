import { Component, OnInit, ViewChild, Injectable } from '@angular/core';
import { UserService } from '../../services/user.service';
import { AlertService } from '../../services/alert.service';
import { User } from '../../models/user';
import { Subscription } from 'rxjs/Subscription';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';

import {Observable} from 'rxjs/Observable';
import {Subject} from 'rxjs/Subject';

import { UserDetailComponent } from '../user-detail/user-detail.component';
import { GenericTableComponent, GtConfig, GtInformation, GtRow/*, GtCustomComponent */} from 'angular-generic-table';

import {GtCustomComponent} from 'angular-generic-table/generic-table/components/gt-custom-component-factory';

export interface StateDictionary {
  [index: number]: {username?: string, role?: string, email?: string};
}

export function deepCopy(dictionary: StateDictionary) {
  const newDictionary: StateDictionary = {};
  Object.keys(dictionary).forEach(key => {
    newDictionary[key] = {
      username: dictionary[key].username,
      role: dictionary[key].role,
      email: dictionary[key].email
    };
  });
  return newDictionary;
}

export interface UpdateFunction {
  (dictionary: StateDictionary): StateDictionary;
}

export interface Row extends GtRow {
  id: number;
  username: string;
  role: string;
  email: string;
}

@Injectable()
export class StateService {
  private updates: Subject<UpdateFunction>;
  private _states: BehaviorSubject<StateDictionary>;

  get states(): Observable<StateDictionary> {
    return this._states.asObservable();
  }

  constructor() {
    this.updates = new Subject<UpdateFunction>();
    this._states = new BehaviorSubject<StateDictionary>({});
    this.updates
      .scan((previousState, apply: UpdateFunction) => apply(previousState), {})
      // .do(dictionary => console.log(`State = ${JSON.stringify(dictionary, null, 2)}`))
      .subscribe(this._states);
  }

  username(id: number, username: string) {
    this.updates.next(dictionary => {
      const newDictionary = deepCopy(dictionary);
      if (!newDictionary[id]) {
        newDictionary[id] = {};
      }
      newDictionary[id].username = username;
      return newDictionary;
    });
  }

  role(id: number, role: string) {
    this.updates.next(dictionary => {
      const newDictionary = deepCopy(dictionary);
      if (!newDictionary[id]) {
        newDictionary[id] = {};
      }
      newDictionary[id].role = role;
      return newDictionary;
    });
  }

  email(id: number, email: string) {
    this.updates.next(dictionary => {
      const newDictionary = deepCopy(dictionary);
      if (!newDictionary[id]) {
        newDictionary[id] = {};
      }
      newDictionary[id].email = email;
      return newDictionary;
    });
  }

}

@Injectable()
export class EditService {
  private _ids = new Subject<number>();

  get ids(): Observable<number> {
    return this._ids.asObservable();
  }

  click(id: number) {
    this._ids.next(id);
  }
}

@Component({
  template: `
    <input *ngIf="edit | async" type="text" class="form-control form-control-sm" name="name" [(ngModel)]="name">
    <span *ngIf="view | async">{{row.username}}</span>
  `
})
export class NameComponent extends GtCustomComponent<Row> implements OnInit {
  edit: Observable<boolean>;
  view: Observable<boolean>;
  private _name: string;

  get name() {
    return this._name;
  }

  set name(value) {
    this._name = value;
    this.saveService.username(this.row.id, value);
  }

  constructor(private editService: EditService,
              private saveService: StateService) {
    super();
  }

  ngOnInit() {
    const source = this.editService.ids
      .startWith(this.row.id)
      .filter(id => id === this.row.id);
    this.edit = source.scan(prev => !prev, true);
    this.view = source.scan(prev => !prev, false);
    this.name = this.row.username;
  }
}

@Component({
  template: `
    <input *ngIf="edit | async" type="text" class="form-control form-control-sm" name="name" [(ngModel)]="name">
    <span *ngIf="view | async">{{row.email}}</span>
  `
})
export class EmailComponent extends GtCustomComponent<Row> implements OnInit {
  edit: Observable<boolean>;
  view: Observable<boolean>;
  private _name: string;

  get name() {
    return this._name;
  }

  set name(value) {
    this._name = value;
    this.saveService.email(this.row.id, value);
  }

  constructor(private editService: EditService,
              private saveService: StateService) {
    super();
  }

  ngOnInit() {
    const source = this.editService.ids
      .startWith(this.row.id)
      .filter(id => id === this.row.id);
    this.edit = source.scan(prev => !prev, true);
    this.view = source.scan(prev => !prev, false);
    this.name = this.row.email;
  }
}

@Component({
  template: `
    <select *ngIf="edit | async" class="form-control form-control-sm" name="role" [(ngModel)]="role">
      <option *ngFor="let ROLE of ROLES" [value]="ROLE" [selected]="ROLE === role">{{ROLE}}</option>
    </select>
    <span *ngIf="view | async">{{row.role}}</span>
  `
})
export class RoleComponent extends GtCustomComponent<Row> implements OnInit {
  ROLES = ['user', 'admin'];
  edit: Observable<boolean>;
  view: Observable<boolean>;
  private _role: string;

  get role(): string {
    return this._role;
  }

  set role(value: string) {
    this._role = value;
    this.saveService.role(this.row.id, value);
  }

  constructor(private editService: EditService,
              private saveService: StateService) {
    super();
  }

  ngOnInit() {
    const source = this.editService.ids
      .startWith(this.row.id)
      .filter(id => id === this.row.id);
    this.edit = source.scan(prev => !prev, true);
    this.view = source.scan(prev => !prev, false);
    this.role = this.row.role;
  }
}

@Component({
    selector: 'users',
    templateUrl: './users.component.html',
    providers: [EditService, StateService]
})
export class UsersComponent implements OnInit {

    public configObject: GtConfig<any>;
    public expandedRow = UserDetailComponent;

    users: User[] = [];

    @ViewChild(GenericTableComponent)
    private myTable: GenericTableComponent<any, UserDetailComponent>;

saveAll() {
    this.stateService.states
      .take(1)
      .subscribe(dictionary => {
        const newData: Row[] = Object.keys(dictionary).map(key => ({
          id: parseInt(key),
          username: dictionary[key].username,
          role: dictionary[key].role,
          email: dictionary[key].email
        }));
        // newData.forEach(row => {
        //   console.log(`Saving name = "${row.name}" and age = ${row.age} for id = ${row.id}`);
        // });
        this.configObject.data = newData;
      });
  }

    constructor(
        private editService: EditService,
        private stateService: StateService,
        private _userService: UserService,
        private _alertService: AlertService) { 

        this.configObject = {
            settings: [{
                objectKey: 'edit',
                columnOrder: 0,
                sort: 'disable'
            }, {
                objectKey: 'username',
                visible: true,
                sort: 'desc',
                columnOrder: 1
            }, {
                objectKey: 'email',
                visible: true,
                enabled: true,
                sort: 'enable',
                sortOrder: 0,
                columnOrder: 2
            }, {
                objectKey: 'role',
                visible: true,
                sort: 'desc',
                columnOrder: 3
            }, {
                objectKey: 'save',
                columnOrder: 4,
                sort: 'disable'
            }, {
                objectKey: 'delete',
                columnOrder: 5,
                sort: 'disable'
            }],
            fields: [{
                objectKey: 'edit', name: '',
                value: () => '',
                render: () => '<button type="button" class="btn btn-primary btn-sm">Edit</button>',
                click: (row) => this.editService.click(row.id)
            }, {
                name: 'Username',
                objectKey: 'username',
                expand: true,
                classNames: 'clickable sort-string',
                columnComponent: {
                    type: NameComponent
                }
            }, {
                name: 'Email',
                objectKey: 'email',
                columnComponent: {
                    type: EmailComponent
                }
            }, {
                name: 'Role',
                objectKey: 'role',
                columnComponent: {
                    type: RoleComponent
                }
            }, {
                objectKey: 'save',
                name: '',
                value: () => '',
                classNames:'text-right',
                render: () => '<button type="button" class="btn btn-success btn-sm">Save</button>',
                click: (row) => this.saveAll()
            }, {
                objectKey: 'delete',
                name: '',
                value: () => '',
                classNames:'text-right',
                render: () => '<button type="button" class="btn btn-danger btn-sm">Delete</button>'/*,
                click: (row) => this.stateService.states
                    .take(1)
                    .delay(Math.floor(Math.random() * 2000) + 1000)
                    .subscribe(dictionary => {
                        const name = dictionary[row.id].name;
                        const age = dictionary[row.id].age;
                        console.log(`Saving name = "${name}" and age = ${age} for id = ${row.id}`);
                        row.name = name;
                        row.age = age;
                    })*/
            }],
            data: []
        };
    }

    ngOnInit() {
        this._userService.getAll()
            .subscribe(
            result => {
                this.users = result;
                this.configObject.data = result;
                // this.configObject.data.length = 0;
                // this.configObject.data = this.configObject.data.concat(data);
                // return this.configObject.data;
            },
            error => {
                this._alertService.error(error);
            });
    }
}
