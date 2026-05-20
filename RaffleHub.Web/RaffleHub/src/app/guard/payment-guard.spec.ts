import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, convertToParamMap } from '@angular/router';
import { paymentGuard } from './payment-guard';
import { AuthService } from '../service/auth-service';
import { AuthCookieService } from '../service/auth-cookie-service';

describe('paymentGuard', () => {
  let mockAuthService: any;
  let mockCookieService: any;
  let mockRouter: any;

  beforeEach(() => {
    mockAuthService = {
      isValidToken: vi.fn()
    };
    mockCookieService = {
      getParticipantId: vi.fn()
    };
    mockRouter = {
      navigate: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: AuthCookieService, useValue: mockCookieService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  const runGuard = (route: Partial<ActivatedRouteSnapshot>) => {
    return TestBed.runInInjectionContext(() => {
      return paymentGuard(route as ActivatedRouteSnapshot, {} as any);
    });
  };

  it('should allow access if the token is valid', () => {
    mockAuthService.isValidToken.mockReturnValue(true);

    const result = runGuard({});

    expect(result).toBe(true);
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('should allow access if route participantId matches cookie participantId', () => {
    mockAuthService.isValidToken.mockReturnValue(false);
    mockCookieService.getParticipantId.mockReturnValue('part-123');

    const mockRoute = {
      paramMap: convertToParamMap({ participantId: 'part-123' })
    };

    const result = runGuard(mockRoute);

    expect(result).toBe(true);
  });

  it('should block access and navigate to root if token is invalid and IDs do not match', () => {
    mockAuthService.isValidToken.mockReturnValue(false);
    mockCookieService.getParticipantId.mockReturnValue('part-cookie');

    const mockRoute = {
      paramMap: convertToParamMap({ participantId: 'part-different' })
    };

    const result = runGuard(mockRoute);

    expect(result).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });
});
