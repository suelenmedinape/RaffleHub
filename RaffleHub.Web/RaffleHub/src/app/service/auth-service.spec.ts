import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { AuthService } from './auth-service';
import { AuthCookieService } from './auth-cookie-service';
import { ApiRoutesService } from '../core/api-routes-service';

describe('AuthService', () => {
  let service: AuthService;

  const mockAuthCookieService = {
    hasToken: vi.fn().mockReturnValue(false),
    getToken: vi.fn(),
    setAuthData: vi.fn(),
    clearAuthData: vi.fn(),
    getUserData: vi.fn()
  };

  const mockApiRoutesService = {
    authUrl: {
      login: '/api/login',
      register: '/api/register'
    }
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthCookieService, useValue: mockAuthCookieService },
        { provide: ApiRoutesService, useValue: mockApiRoutesService }
      ]
    });
    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
